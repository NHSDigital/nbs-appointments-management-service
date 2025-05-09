using Nhs.Appointments.Core.Concurrency;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Core;

public interface IBookingWriteService
{
    Task<(bool Success, string Reference)> MakeBooking(Booking booking);

    Task<BookingCancellationResult> CancelBooking(string bookingReference, string site);

    Task<bool> SetBookingStatus(string bookingReference, AppointmentStatus status,
        AvailabilityStatus availabilityStatus);

    Task SendBookingReminders();

    Task<BookingConfirmationResult> ConfirmProvisionalBookings(string[] bookingReferences,
        IEnumerable<ContactItem> contactDetails);

    Task<BookingConfirmationResult> ConfirmProvisionalBooking(string bookingReference,
        IEnumerable<ContactItem> contactDetails, string bookingToReschedule);

    Task<IEnumerable<string>> RemoveUnconfirmedProvisionalBookings();

    Task RecalculateAppointmentStatuses(string site, DateOnly day);
}

public class BookingWriteService(
    IBookingsDocumentStore bookingDocumentStore,
    IBookingQueryService bookingQueryService,
    IReferenceNumberProvider referenceNumberProvider,
    ISiteLeaseManager siteLeaseManager,
    IAvailabilityStore availabilityStore,
    IAvailabilityCalculator availabilityCalculator,
    IAllocationStateService allocationStateService,
    IBookingEventFactory eventFactory,
    IMessageBus bus,
    TimeProvider time,
    IFeatureToggleHelper featureToggleHelper) : IBookingWriteService
{
    public async Task<(bool Success, string Reference)> MakeBooking(Booking booking)
    {
        if (await featureToggleHelper.IsFeatureEnabled(Flags.MultipleServices))
        {
            return await MakeBooking_MultipleServices(booking);
        }

        return await MakeBooking_SingleService(booking);
    }

    public async Task<BookingCancellationResult> CancelBooking(string bookingReference, string site)
    {
        if (await featureToggleHelper.IsFeatureEnabled(Flags.MultipleServices))
        {
            return await CancelBooking_MultipleServices(bookingReference, site);
        }

        return await CancelBooking_SingleService(bookingReference, site);
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

    public async Task<BookingConfirmationResult> ConfirmProvisionalBooking(string bookingReference,
        IEnumerable<ContactItem> contactDetails, string bookingToReschedule)
    {
        var isRescheduleOperation = !string.IsNullOrEmpty(bookingToReschedule);

        var result =
            await bookingDocumentStore.ConfirmProvisional(bookingReference, contactDetails, bookingToReschedule);

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

        var bookings = await bookingQueryService.GetBookings(windowStart, windowEnd);
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
        if (await featureToggleHelper.IsFeatureEnabled(Flags.MultipleServices))
        {
            await RecalculateAppointmentStatuses_MultipleServices(site, day);
        }

        await RecalculateAppointmentStatuses_SingleService(site, day);
    }

    private async Task<(bool Success, string Reference)> MakeBooking_SingleService(Booking booking)
    {
        using (var leaseContent = siteLeaseManager.Acquire(booking.Site))
        {
#pragma warning disable CS0618 // Keep availabilityCalculator around until MultipleServicesEnabled is stable
            var slots = (await availabilityCalculator.CalculateAvailability(booking.Site, booking.Service,
                    DateOnly.FromDateTime(booking.From.Date), DateOnly.FromDateTime(booking.From.Date.AddDays(1))))
                .ToList();
#pragma warning restore CS0618 // Keep availabilityCalculator around until MultipleServicesEnabled is stable

            var canBook = slots.Any(sl => sl.From == booking.From && sl.Duration.TotalMinutes == booking.Duration);

            if (canBook)
            {
                booking.Created = time.GetUtcNow();
                booking.Reference = await referenceNumberProvider.GetReferenceNumber(booking.Site);
                booking.ReminderSent = false;
                booking.AvailabilityStatus = AvailabilityStatus.Supported;
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

    private async Task<(bool Success, string Reference)> MakeBooking_MultipleServices(Booking booking)
    {
        using (var leaseContent = siteLeaseManager.Acquire(booking.Site))
        {
            var from = booking.From;
            var to = booking.From.AddMinutes(booking.Duration);

            var slots = (await allocationStateService.BuildAllocation(booking.Site, from, to))
                .AvailableSlots;

            var canBook = slots.Any(sl => sl.Services.Contains(booking.Service) && sl.From == booking.From && sl.Duration.TotalMinutes == booking.Duration);

            if (canBook)
            {
                booking.Created = time.GetUtcNow();
                booking.Reference = await referenceNumberProvider.GetReferenceNumber(booking.Site);
                booking.ReminderSent = false;
                booking.AvailabilityStatus = AvailabilityStatus.Supported;
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

    private Task<bool> UpdateAvailabilityStatus(string bookingReference, AvailabilityStatus status) =>
        bookingDocumentStore.UpdateAvailabilityStatus(bookingReference, status);

    private Task DeleteBooking(string reference, string site) => bookingDocumentStore.DeleteBooking(reference, site);

    private async Task<BookingCancellationResult> CancelBooking_SingleService(string bookingReference, string siteId)
    {
        var booking = await bookingDocumentStore.GetByReferenceOrDefaultAsync(bookingReference);

        if (booking is null || (!string.IsNullOrEmpty(siteId) && siteId != booking.Site))
        {
            return BookingCancellationResult.NotFound;
        }

        await bookingDocumentStore.UpdateStatus(bookingReference, AppointmentStatus.Cancelled,
            AvailabilityStatus.Unknown);

        await RecalculateAppointmentStatuses_SingleService(booking.Site, DateOnly.FromDateTime(booking.From));

        if (booking.ContactDetails != null)
        {
            var bookingCancelledEvents = eventFactory.BuildBookingEvents<BookingCancelled>(booking);
            await bus.Send(bookingCancelledEvents);
        }

        return BookingCancellationResult.Success;
    }

    private async Task<BookingCancellationResult> CancelBooking_MultipleServices(string bookingReference, string siteId)
    {
        var booking = await bookingDocumentStore.GetByReferenceOrDefaultAsync(bookingReference);

        if (booking is null || (!string.IsNullOrEmpty(siteId) && siteId != booking.Site))
        {
            return BookingCancellationResult.NotFound;
        }

        await bookingDocumentStore.UpdateStatus(bookingReference, AppointmentStatus.Cancelled,
            AvailabilityStatus.Unknown);

        await RecalculateAppointmentStatuses_MultipleServices(booking.Site, DateOnly.FromDateTime(booking.From));

        if (booking.ContactDetails != null)
        {
            var bookingCancelledEvents = eventFactory.BuildBookingEvents<BookingCancelled>(booking);
            await bus.Send(bookingCancelledEvents);
        }

        return BookingCancellationResult.Success;
    }

    private async Task RecalculateAppointmentStatuses_SingleService(string site, DateOnly day)
    {
        var dayStart = day.ToDateTime(new TimeOnly(0, 0));
        var dayEnd = day.ToDateTime(new TimeOnly(23, 59));

        var bookings = (await bookingQueryService.GetBookings(dayStart, dayEnd, site))
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
                                                        sl.From == booking.From &&
                                                        (int)sl.Duration.TotalMinutes == booking.Duration &&
                                                        sl.Services.Contains(booking.Service));

            if (targetSlot != null)
            {
                if (booking.AvailabilityStatus is not AvailabilityStatus.Supported)
                {
                    await bookingDocumentStore.UpdateAvailabilityStatus(booking.Reference,
                        AvailabilityStatus.Supported);
                }

                targetSlot.Capacity--;
                continue;
            }

            // TODO: Delete the provisional rather than just excluding it from this check. This work is another PR in code review.
            if (booking.AvailabilityStatus is AvailabilityStatus.Supported &&
                booking.Status is not AppointmentStatus.Provisional)
            {
                await bookingDocumentStore.UpdateAvailabilityStatus(booking.Reference, AvailabilityStatus.Orphaned);
            }

            if (booking.Status is AppointmentStatus.Provisional)
            {
                await bookingDocumentStore.DeleteBooking(booking.Reference, booking.Site);
            }
        }
    }

    private async Task RecalculateAppointmentStatuses_MultipleServices(string site, DateOnly day)
    {
        var dayStart = day.ToDateTime(new TimeOnly(0, 0));
        var dayEnd = day.ToDateTime(new TimeOnly(23, 59));

        var recalculations = await allocationStateService.BuildRecalculations(site, dayStart, dayEnd);

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
