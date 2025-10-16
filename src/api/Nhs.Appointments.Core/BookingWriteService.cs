using Newtonsoft.Json.Linq;
using Nhs.Appointments.Core.Concurrency;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Core;

public interface IBookingWriteService
{
    Task<(bool Success, string Reference)> MakeBooking(Booking booking);

    Task<BookingCancellationResult> CancelBooking(
        string bookingReference, 
        string site,
        CancellationReason cancellationReason, 
        object additionalData, 
        bool runRecalculation = true);

    Task<bool> SetBookingStatus(string bookingReference, AppointmentStatus status,
        AvailabilityStatus availabilityStatus);

    Task SendBookingReminders();

    Task<BookingConfirmationResult> ConfirmProvisionalBookings(string[] bookingReferences,
        IEnumerable<ContactItem> contactDetails);

    Task<BookingConfirmationResult> ConfirmProvisionalBooking(string bookingReference,
        IEnumerable<ContactItem> contactDetails, string bookingToReschedule);

    Task<IEnumerable<string>> RemoveUnconfirmedProvisionalBookings();

    Task RecalculateAppointmentStatuses(string site, DateOnly day, bool cancelUnsupportedBookings = false);
    Task RecalculateAppointmentStatuses(string site, DateOnly[] days, bool cancelUnsupportedBookings = false);

    Task<(int cancelledBookingsCount, int bookingsWithoutContactDetailsCount)> CancelAllBookingsInDayAsync(string site, DateOnly day);

    Task SendAutoCancelledBookingNotifications();
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

    public async Task<BookingCancellationResult> CancelBooking(string bookingReference, string site,
        CancellationReason cancellationReason, object additionalData = null, bool runRecalculation = true)
    {
        var booking = await bookingDocumentStore.GetByReferenceOrDefaultAsync(bookingReference);

        if (booking is null || (!string.IsNullOrEmpty(site) && site != booking.Site))
        {
            return BookingCancellationResult.NotFound;
        }

        var mergedAdditionalData = MergeAdditionalData(booking.AdditionalData, additionalData);

        await bookingDocumentStore.UpdateStatus(
            bookingReference, 
            AppointmentStatus.Cancelled,
            AvailabilityStatus.Unknown,
            cancellationReason,
            mergedAdditionalData
        );

        if (runRecalculation)
        {
            await RecalculateAppointmentStatuses(booking.Site, DateOnly.FromDateTime(booking.From));
        }

        await RaiseBookingCancelledNotificationEvents(booking, cancellationReason, mergedAdditionalData);

        return BookingCancellationResult.Success;
    }

    private async Task RaiseBookingCancelledNotificationEvents(Booking booking, CancellationReason cancellationReason,
        object additionalData = null)
    {
        if (booking?.ContactDetails == null)
        {
            return;
        }

        var autoCancellationProp = ExtractField<bool>(additionalData, "AutoCancellation");
        if (cancellationReason == CancellationReason.CancelledByService && autoCancellationProp)
        {
            var bookingAutoCancelledEvents = eventFactory.BuildBookingEvents<BookingAutoCancelled>(booking);
            await bus.Send(bookingAutoCancelledEvents);
        }
        else
        {
            var bookingCancelledEvents = eventFactory.BuildBookingEvents<BookingCancelled>(booking);
            await bus.Send(bookingCancelledEvents);
        }

        await bookingDocumentStore.SetCancellationNotified(booking.Reference, booking.Site);
    }

    private object ConvertJTokensBackToDictionaries(JToken token)
    {
        switch (token.Type)
        {
            case JTokenType.Object:
                return token.Children<JProperty>()
                    .ToDictionary(prop => prop.Name, prop => ConvertJTokensBackToDictionaries(prop.Value));
            case JTokenType.Array:
                return token.Select(ConvertJTokensBackToDictionaries).ToList();
            case JTokenType.Null:
                return null;
            default:
                return ((JValue)token).Value;
        }
    }

    private T ExtractField<T>(object additionalData, string fieldName)
    {
        var current = additionalData != null
            ? JObject.FromObject(additionalData)
            : new JObject();
        var fieldValue = current.GetValue(fieldName);

        return fieldValue != null ? fieldValue.ToObject<T>() : default;
    }

    public object MergeAdditionalData(object currentAdditionalData, object incomingAdditionalData)
    {
        var current = currentAdditionalData != null
            ? JObject.FromObject(currentAdditionalData)
            : new JObject();

        var incoming = incomingAdditionalData != null
            ? JObject.FromObject(incomingAdditionalData)
            : new JObject();

        current.Merge(incoming, new JsonMergeSettings
        {
            MergeArrayHandling = MergeArrayHandling.Replace, MergeNullValueHandling = MergeNullValueHandling.Merge
        });

        return current.HasValues ? ConvertJTokensBackToDictionaries(current) : null;
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

    public async Task RecalculateAppointmentStatuses(string site, DateOnly day, bool cancelUnsupportedBookings = false)
    {
        using var leaseContent = siteLeaseManager.Acquire(site);

        await RecalculateAppointmentStatusesForDay(site, day, cancelUnsupportedBookings);
    }

    public async Task RecalculateAppointmentStatuses(string site, DateOnly[] days, bool cancelUnsupportedBookings = false)
    {
        using var leaseContent = siteLeaseManager.Acquire(site);

        var dayTasks = days.Select(day => RecalculateAppointmentStatusesForDay(site, day, cancelUnsupportedBookings));

        await Task.WhenAll(dayTasks);
    }

    private async Task RecalculateAppointmentStatusesForDay(string site, DateOnly day, bool cancelUnsupportedBookings = false)
    {
        var dayStart = day.ToDateTime(new TimeOnly(0, 0));
        var dayEnd = day.ToDateTime(new TimeOnly(23, 59));

        var recalculations = await bookingAvailabilityStateService.BuildRecalculations(site, dayStart, dayEnd);

        foreach (var update in recalculations)
        {
            switch (update.Action)
            {
                case AvailabilityUpdateAction.ProvisionalToDelete:
                    await DeleteBooking(update.Booking.Reference, update.Booking.Site);
                    break;

                case AvailabilityUpdateAction.SetToOrphaned:
                    await UpdateAvailabilityStatus(update.Booking.Reference, AvailabilityStatus.Orphaned);
                    if (cancelUnsupportedBookings)
                    {
                        await CancelBooking(update.Booking.Reference, update.Booking.Site, CancellationReason.CancelledBySite, runRecalculation: false);
                    }
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
    {
        var (cancelledBookingsCount, bookingsWithoutContactDetailsCount, bookingsWithContactDetails) = await bookingDocumentStore.CancelAllBookingsInDay(site, day);

        foreach (var booking in bookingsWithContactDetails)
        {
            var bookingCancelledEvents = eventFactory.BuildBookingEvents<BookingCancelled>(booking);
            await bus.Send(bookingCancelledEvents);
        }

        return (cancelledBookingsCount, bookingsWithoutContactDetailsCount);
    }

    public async Task SendAutoCancelledBookingNotifications()
    {
        var now = time.GetLocalNow().DateTime;
        var windowStart = now.AddDays(-1);
        var windowEnd = now;

        var bookingsCancelledByService = (await bookingQueryService.GetRecentlyUpdatedBookingsCrossSiteAsync(windowStart, windowEnd)).Where(
            b => b.CancellationReason == CancellationReason.CancelledByService &&
            (b.CancellationNotificationStatus == CancellationNotificationStatus.Unnotified || b.CancellationNotificationStatus is null) &&
            b.ContactDetails is not null &&
            b.ContactDetails.Length > 0).ToList();

        if (bookingsCancelledByService.Count == 0)
        {
            return;
        }

        var autoCancelledBookings = bookingsCancelledByService.Where(b =>
        {
            if (b.AdditionalData is null)
            {
                return false;
            }

            var type = b.AdditionalData.GetType();
            var autoCancellationProp = type.GetProperty("AutoCancellation", typeof(bool));
            return autoCancellationProp is not null &&
                   autoCancellationProp.GetValue(b.AdditionalData) is bool value &&
                   value;
        }).ToList();

        if (autoCancelledBookings.Count == 0)
        {
            return;
        }

        foreach (var booking in autoCancelledBookings)
        {
            var notifcations = eventFactory.BuildBookingEvents<BookingAutoCancelled>(booking);
            await bus.Send(notifcations);
            booking.CancellationNotificationStatus = CancellationNotificationStatus.Notified;
            await bookingDocumentStore.SetCancellationNotified(booking.Reference, booking.Site);
        }
    }

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
