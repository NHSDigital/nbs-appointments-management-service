using Nhs.Appointments.Core.Concurrency;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Core;

public interface IBookingWriteService
{
    Task<(bool Success, string Reference)> MakeBooking(Booking booking);

    Task<BookingCancellationResult> CancelBooking(string bookingReference, string site, CancellationReason cancellationReason);

    Task<bool> SetBookingStatus(string bookingReference, AppointmentStatus status,
        AvailabilityStatus availabilityStatus);

    Task SendBookingReminders();

    Task<BookingConfirmationResult> ConfirmProvisionalBookings(string[] bookingReferences,
        IEnumerable<ContactItem> contactDetails);

    Task<BookingConfirmationResult> ConfirmProvisionalBooking(string bookingReference,
        IEnumerable<ContactItem> contactDetails, string bookingToReschedule);

    Task<IEnumerable<string>> RemoveUnconfirmedProvisionalBookings();

    Task RecalculateAppointmentStatuses(string site, DateOnly day);

    Task<(int cancelledBookingsCount, int bookingsWithoutContactDetailsCount)> CancelAllBookingsInDayAsync(string site, DateOnly day);
}

public class BookingWriteService(
    IBookingsDocumentStore bookingDocumentStore,
    IBookingQueryService bookingQueryService,
    IReferenceNumberProvider referenceNumberProvider,
    ISiteLeaseManager siteLeaseManager,
    IBookingAvailabilityStateService bookingAvailabilityStateService,
    IBookingEventFactory eventFactory,
    IMessageBus bus,
    TimeProvider time) : IBookingWriteService
{
    public async Task<(bool Success, string Reference)> MakeBooking(Booking booking)
    {
        using var leaseContent = siteLeaseManager.Acquire(booking.Site);
        
        var from = booking.From;
        var to = booking.From.AddMinutes(booking.Duration);

        var availableSlots = await bookingAvailabilityStateService.GetAvailableSlots(booking.Site, from, to);

        //duration totalMinutes should always be an integer until we allow slot lengths that aren't integer minutes
        var canBook = availableSlots.Any(sl => sl.Services.Contains(booking.Service) && sl.From == booking.From && (int)sl.Duration.TotalMinutes == booking.Duration);

        if (!canBook)
        {
            return (false, string.Empty);
        }

        booking.Created = time.GetUtcNow();
        booking.Reference = await referenceNumberProvider.GetReferenceNumber(booking.Site);
        booking.ReminderSent = false;
        booking.AvailabilityStatus = AvailabilityStatus.Supported;
        await bookingDocumentStore.InsertAsync(booking);

        if (booking.Status != AppointmentStatus.Booked || !(booking.ContactDetails?.Length > 0))
        {
            return (true, booking.Reference);
        }

        var bookingMadeEvents = eventFactory.BuildBookingEvents<BookingMade>(booking);
        await bus.Send(bookingMadeEvents);

        return (true, booking.Reference);
    }

    public async Task<BookingCancellationResult> CancelBooking(string bookingReference, string site, CancellationReason cancellationReason)
    {
        var booking = await bookingDocumentStore.GetByReferenceOrDefaultAsync(bookingReference);

        if (booking is null || (!string.IsNullOrEmpty(site) && site != booking.Site))
        {
            return BookingCancellationResult.NotFound;
        }

        await bookingDocumentStore.UpdateStatus(
            bookingReference, 
            AppointmentStatus.Cancelled,
            AvailabilityStatus.Unknown, 
            cancellationReason
        );

        await RecalculateAppointmentStatuses(booking.Site, DateOnly.FromDateTime(booking.From));

        if (booking.ContactDetails != null)
        {
            var bookingCancelledEvents = eventFactory.BuildBookingEvents<BookingCancelled>(booking);
            await bus.Send(bookingCancelledEvents);
        }

        return BookingCancellationResult.Success;
    }
    
    public async Task<BookingConfirmationResult> ConfirmProvisionalBookings(string[] bookingReferences,
        IEnumerable<ContactItem> contactDetails)
    {
        var result = await bookingDocumentStore.ConfirmProvisionals(bookingReferences, contactDetails);

        foreach (var booking in bookingReferences)
        {
            await SendConfirmNotification(booking, result, false);
        }

        return result;
    }

    public async Task<BookingConfirmationResult> ConfirmProvisionalBooking(string bookingReference, IEnumerable<ContactItem> contactDetails, string bookingToReschedule)
    {
        var isRescheduleOperation = !string.IsNullOrEmpty(bookingToReschedule);

        var result = isRescheduleOperation
            ? await bookingDocumentStore.ConfirmProvisional(bookingReference, contactDetails, bookingToReschedule, CancellationReason.RescheduledByCitizen)
            : await bookingDocumentStore.ConfirmProvisional(bookingReference, contactDetails, bookingToReschedule);

        await SendConfirmNotification(bookingReference, result, isRescheduleOperation);

        return result;
    }

    public Task<bool> SetBookingStatus(string bookingReference, AppointmentStatus status,
        AvailabilityStatus availabilityStatus) =>
        bookingDocumentStore.UpdateStatus(bookingReference, status, availabilityStatus);

    public async Task SendBookingReminders()
    {
        var windowStart = time.GetLocalNow().DateTime;
        var windowEnd = windowStart.AddDays(3);

        var bookings = await bookingQueryService.GetBookedBookingsAcrossAllSites(windowStart, windowEnd);
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

        var recalculations = await bookingAvailabilityStateService.BuildRecalculations(site, dayStart, dayEnd);

        using var leaseContent = siteLeaseManager.Acquire(site);

        foreach (var update in recalculations)
        {
            switch (update.Action)
            {
                case AvailabilityUpdateAction.ProvisionalToDelete:
                    await DeleteBooking(update.Booking.Reference, update.Booking.Site);
                    break;

                case AvailabilityUpdateAction.SetToOrphaned:
                    await UpdateAvailabilityStatus(update.Booking.Reference,
                        AvailabilityStatus.Orphaned);
                    break;

                case AvailabilityUpdateAction.SetToSupported:
                    await UpdateAvailabilityStatus(update.Booking.Reference,
                        AvailabilityStatus.Supported);
                    break;

                case AvailabilityUpdateAction.Default:
                default:
                    break;
            }
        }
    }

    public async Task<(int cancelledBookingsCount, int bookingsWithoutContactDetailsCount)> CancelAllBookingsInDayAsync(string site, DateOnly day)
        => await bookingDocumentStore.CancelAllBookingsInDay(site, day);

    private Task<bool> UpdateAvailabilityStatus(string bookingReference, AvailabilityStatus status) =>
        bookingDocumentStore.UpdateAvailabilityStatus(bookingReference, status);

    private Task DeleteBooking(string reference, string site) => bookingDocumentStore.DeleteBooking(reference, site);

    private async Task SendConfirmNotification(string bookingReference, BookingConfirmationResult result,
        bool isRescheduleOperation)
    {
        if (result == BookingConfirmationResult.Success)
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
    }
}
