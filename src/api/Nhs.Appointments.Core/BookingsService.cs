using Nhs.Appointments.Core.Concurrency;
using Nhs.Appointments.Core.Messaging;

namespace Nhs.Appointments.Core;

public interface IBookingsService
{
    Task<IEnumerable<Booking>> GetBookings(DateTime from, DateTime to, string site);
    Task<Booking> GetBookingByReference(string bookingReference);
    Task<IEnumerable<Booking>> GetBookingByNhsNumber(string nhsNumber);
    Task<(bool Success, string Reference, bool Provisional)> MakeBooking(Booking booking);
    Task<BookingCancellationResult> CancelBooking(string bookingReference);
    Task<bool> SetBookingStatus(string bookingReference, string status);
    Task SendBookingReminders();
    Task<BookingConfirmationResult> ConfirmProvisionalBooking(string bookingReference, IEnumerable<ContactItem> contactDetails, string bookingToReschedule);
    Task<IEnumerable<string>> RemoveUnconfirmedProvisionalBookings();
}    

public class BookingsService(
        IBookingsDocumentStore bookingDocumentStore,
        IReferenceNumberProvider referenceNumberProvider,
        ISiteLeaseManager siteLeaseManager,
        IAvailabilityCalculator availabilityCalculator,
        IBookingEventFactory eventFactory,
        IMessageBus bus,
        TimeProvider time) : IBookingsService
{ 
    public Task<IEnumerable<Booking>> GetBookings(DateTime from, DateTime to, string site)
    {
        return bookingDocumentStore.GetInDateRangeAsync(from, to, site);
    }

    protected Task<IEnumerable<Booking>> GetBookings(DateTime from, DateTime to)
    {
        return bookingDocumentStore.GetCrossSiteAsync(from, to, false);
    }

    public Task<Booking> GetBookingByReference(string bookingReference)
    {
        return bookingDocumentStore.GetByReferenceOrDefaultAsync(bookingReference);
    }
    
    public Task<IEnumerable<Booking>> GetBookingByNhsNumber(string nhsNumber)
    {
        return bookingDocumentStore.GetByNhsNumberAsync(nhsNumber);
    }

    public async Task<(bool Success, string Reference, bool Provisional)> MakeBooking(Booking booking)
    {            
        using (var leaseContent = siteLeaseManager.Acquire(booking.Site))
        {                
            var slots = await availabilityCalculator.CalculateAvailability(booking.Site, booking.Service, DateOnly.FromDateTime(booking.From.Date), DateOnly.FromDateTime(booking.From.Date.AddDays(1)));
            var canBook = slots.Any(sl => sl.From == booking.From && sl.Duration.TotalMinutes == booking.Duration);

            if (canBook)
            {
                booking.Created = time.GetUtcNow().DateTime;
                booking.Reference = await referenceNumberProvider.GetReferenceNumber(booking.Site);
                booking.ReminderSent = false;
                await bookingDocumentStore.InsertAsync(booking);

                if (!booking.Provisional)
                {
                    var bookingMadeEvent = eventFactory.BuildBookingMadeEvent(booking);
                    await bus.Send(bookingMadeEvent);
                }

                return (true, booking.Reference, booking.Provisional);
            }

            return (false, string.Empty, booking.Provisional);
        }            
    }

    public async Task<BookingCancellationResult> CancelBooking(string bookingReference)
    {
        var booking = await bookingDocumentStore.GetByReferenceOrDefaultAsync(bookingReference);

        if (booking == null)
        {
            return BookingCancellationResult.NotFound;
        }

        await bookingDocumentStore
            .BeginUpdate(booking.Site, bookingReference)
            .UpdateProperty(b => b.Outcome, "Cancelled")                
            .ApplyAsync();

        var bookingCancelledEvent = eventFactory.BuildBookingCancelledEvent(booking);
        await bus.Send (bookingCancelledEvent);

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
                var bookingRescheduledEvent = eventFactory.BuildBookingRescheduledEvent(booking);
                await bus.Send(bookingRescheduledEvent);
            }
            else
            {
                var bookingMadeEvent = eventFactory.BuildBookingMadeEvent(booking);
                await bus.Send(bookingMadeEvent);
            }
        }

        return result;
    }

    public Task<bool> SetBookingStatus(string bookingReference, string status)
    {
        return bookingDocumentStore.UpdateStatus(bookingReference, status);
    }

    public async Task SendBookingReminders()
    {
        var bookings = await GetBookings(time.GetLocalNow().DateTime, time.GetLocalNow().AddDays(3).DateTime);
        foreach (var booking in bookings.Where(b => !b.ReminderSent))
        {
            var reminder = eventFactory.BuildBookingReminderEvent(booking);
            await bus.Send(reminder);
            booking.ReminderSent = true;
            await bookingDocumentStore.SetReminderSent(booking.Reference, booking.Site);
        }
    }

    public Task<IEnumerable<string>> RemoveUnconfirmedProvisionalBookings()
    {
        return bookingDocumentStore.RemoveUnconfirmedProvisionalBookings();
    }
}