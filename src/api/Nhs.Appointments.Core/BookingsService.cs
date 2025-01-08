using Nhs.Appointments.Core.Concurrency;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Core;

public interface IBookingsService
{
    Task<IEnumerable<Booking>> GetBookings(DateTime from, DateTime to, string site);
    Task<Booking> GetBookingByReference(string bookingReference);
    Task<IEnumerable<Booking>> GetBookingByNhsNumber(string nhsNumber);
    Task<(bool Success, string Reference)> MakeBooking(Booking booking);
    Task<BookingCancellationResult> CancelBooking(string bookingReference, string site);
    Task<bool> SetBookingStatus(string bookingReference, AppointmentStatus status);
    Task SendBookingReminders();
    Task<BookingConfirmationResult> ConfirmProvisionalBooking(string bookingReference, IEnumerable<ContactItem> contactDetails, string bookingToReschedule);
    Task<IEnumerable<string>> RemoveUnconfirmedProvisionalBookings();
    Task RecalculateAppointmentStatuses(string site, DateOnly day);
}    

public class BookingsService(
        IBookingsDocumentStore bookingDocumentStore,
        IReferenceNumberProvider referenceNumberProvider,
        ISiteLeaseManager siteLeaseManager,
        IAvailabilityCalculator availabilityCalculator,
        IAvailabilityStore availabilityStore,
        IBookingEventFactory eventFactory,
        IMessageBus bus,
        TimeProvider time) : IBookingsService
{ 
    public async Task<IEnumerable<Booking>> GetBookings(DateTime from, DateTime to, string site)
    {
        var bookings = await bookingDocumentStore.GetInDateRangeAsync(from, to, site);
        return bookings
            .OrderBy(b => b.From)
            .ThenBy(b => b.AttendeeDetails.LastName);
    }

    protected Task<IEnumerable<Booking>> GetBookings(DateTime from, DateTime to)
    {
        return bookingDocumentStore.GetCrossSiteAsync(from, to, AppointmentStatus.Booked);
    }

    public Task<Booking> GetBookingByReference(string bookingReference)
    {
        return bookingDocumentStore.GetByReferenceOrDefaultAsync(bookingReference);
    }
    
    public Task<IEnumerable<Booking>> GetBookingByNhsNumber(string nhsNumber)
    {
        return bookingDocumentStore.GetByNhsNumberAsync(nhsNumber);
    }

    public async Task<(bool Success, string Reference)> MakeBooking(Booking booking)
    {            
        using (var leaseContent = siteLeaseManager.Acquire(booking.Site))
        {                
            var slots = await availabilityCalculator.CalculateAvailability(booking.Site, booking.Service, DateOnly.FromDateTime(booking.From.Date), DateOnly.FromDateTime(booking.From.Date.AddDays(1)));
            var canBook = slots.Any(sl => sl.From == booking.From && sl.Duration.TotalMinutes == booking.Duration);

            if (canBook)
            {
                booking.Created = time.GetUtcNow();                
                booking.Reference = await referenceNumberProvider.GetReferenceNumber(booking.Site);
                booking.ReminderSent = false;
                await bookingDocumentStore.InsertAsync(booking);

                if (booking.Status == AppointmentStatus.Booked && booking.ContactDetails?.Length > 0)
                {
                    var bookingMadeEvents = eventFactory.BuildBookingEvents<BookingMade>(booking);
                    await bus.Send(bookingMadeEvents);
                }

                return (true, booking.Reference);
            }

            return (false, string.Empty);
        }            
    }

    public async Task<BookingCancellationResult> CancelBooking(string bookingReference, string siteId)
    {
        var booking = await bookingDocumentStore.GetByReferenceOrDefaultAsync(bookingReference);

        if (booking is null || (!string.IsNullOrEmpty(siteId) && siteId != booking.Site))
        {
            return BookingCancellationResult.NotFound;
        }

        await bookingDocumentStore.UpdateStatus(bookingReference, AppointmentStatus.Cancelled);
        await RecalculateAppointmentStatuses(booking.Site, DateOnly.FromDateTime(booking.From));

        if (booking.ContactDetails != null)
        {
            var bookingCancelledEvents = eventFactory.BuildBookingEvents<BookingCancelled>(booking);
            await bus.Send(bookingCancelledEvents);
        }

        return BookingCancellationResult.Success;
    }

    public async Task<BookingConfirmationResult> ConfirmProvisionalBooking(string bookingReference, IEnumerable<ContactItem> contactDetails, string bookingToReschedule)
    {
        var isRescheduleOperation = !string.IsNullOrEmpty(bookingToReschedule);

        var result = await bookingDocumentStore.ConfirmProvisional(bookingReference, contactDetails, bookingToReschedule);

        if(result == BookingConfirmationResult.Success)
        {
            var booking = await bookingDocumentStore.GetByReferenceOrDefaultAsync(bookingReference);

            if (isRescheduleOperation)
            {
                var bookingRescheduledEvents = eventFactory.BuildBookingEvents<BookingRescheduled>(booking);
                await bus.Send(bookingRescheduledEvents);
            }
            else
            {
                var bookingMadeEvents = eventFactory.BuildBookingEvents<BookingMade>(booking);
                await bus.Send(bookingMadeEvents);
            }
        }

        return result;
    }

    public Task<bool> SetBookingStatus(string bookingReference, AppointmentStatus status)
    {
        return bookingDocumentStore.UpdateStatus(bookingReference, status);
    }

    public async Task SendBookingReminders()
    {
        var windowStart = time.GetLocalNow().DateTime;
        var windowEnd = windowStart.AddDays(3);

        var bookings = await GetBookings(windowStart, windowEnd);
        foreach (var booking in bookings.Where(b => !b.ReminderSent && b.Created < windowStart.AddDays(-1)))
        {
            var reminders = eventFactory.BuildBookingEvents<BookingReminder>(booking);
            await bus.Send(reminders);
            booking.ReminderSent = true;
            await bookingDocumentStore.SetReminderSent(booking.Reference, booking.Site);
        }
    }

    public Task<IEnumerable<string>> RemoveUnconfirmedProvisionalBookings()
    {
        return bookingDocumentStore.RemoveUnconfirmedProvisionalBookings();
    }

    public async Task RecalculateAppointmentStatuses(string site, DateOnly day)
    {
        var dayStart = day.ToDateTime(new TimeOnly(0, 0));
        var dayEnd = day.ToDateTime(new TimeOnly(23, 59));

        var bookings = (await GetBookings(dayStart, dayEnd, site))
            .Where(b => b.Status is not AppointmentStatus.Cancelled)
            .OrderBy(b => b.Created);

        var sessionsOnThatDay =
            (await availabilityStore.GetSessions(site, day, day))
            .ToList();

        var slots = sessionsOnThatDay.SelectMany(session => session.ToSlots()).ToList();

        using var leaseContent = siteLeaseManager.Acquire(site);
        
        foreach (var booking in bookings)
        {            
            var targetSlot = slots.FirstOrDefault(sl => sl.Capacity > 0 &&
                sl.From == booking.From && (int)sl.Duration.TotalMinutes == booking.Duration && sl.Services.Contains(booking.Service));

            if (targetSlot != null)
            {
                if (booking.Status != AppointmentStatus.Booked)
                {
                    await SetBookingStatus(booking.Reference, AppointmentStatus.Booked);
                    booking.Status = AppointmentStatus.Booked;
                }
                targetSlot.Capacity--;
                continue;
            }

            if (booking.Status is AppointmentStatus.Booked or AppointmentStatus.Provisional)
            {
                await SetBookingStatus(booking.Reference, AppointmentStatus.Orphaned);
            }
        }
    }
}
