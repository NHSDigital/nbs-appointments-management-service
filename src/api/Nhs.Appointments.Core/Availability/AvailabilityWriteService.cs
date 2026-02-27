using System.Globalization;
using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Core.Concurrency;

namespace Nhs.Appointments.Core.Availability;

public class AvailabilityWriteService(
    IAvailabilityStore availabilityStore,
    IAvailabilityCreatedEventStore availabilityCreatedEventStore,
    IBookingWriteService bookingWriteService,
    ISiteLeaseManager siteLeaseManager) : IAvailabilityWriteService
{
    public async Task ApplyAvailabilityTemplateAsync(string site, DateOnly from, DateOnly until, Template template, ApplyAvailabilityMode mode, string user)
    {
        if (string.IsNullOrEmpty(site))
        {
            throw new ArgumentException("Site must have a value.");
        }

        if (from > until)
        {
            throw new ArgumentException("Until date must be after from date.");
        }

        if (template == null)
        {
            throw new ArgumentException("Template must be provided.");
        }

        if (template.Sessions is null || template.Sessions.Length == 0)
        {
            throw new ArgumentException("Template must contain one or more sessions.");
        }

        if (template.Days is null || template.Days.Length == 0)
        {
            throw new ArgumentException("Template must specify one or more weekdays.");
        }

        var dates = GetDatesBetween(from, until, template.Days);
        foreach (var date in dates)
        {
            await SetAvailabilityAsync(date, site, template.Sessions, mode);
        }

        await availabilityCreatedEventStore.LogTemplateCreated(site, from, until, template, user);
    }

    public async Task ApplySingleDateSessionAsync(DateOnly date, string site, Session[] sessions,
        ApplyAvailabilityMode mode, string user, Session sessionToEdit = null)
    {
        await SetAvailabilityAsync(date, site, sessions, mode, sessionToEdit);
        await availabilityCreatedEventStore.LogSingleDateSessionCreated(site, date, sessions, user);
    }

    public async Task SetAvailabilityAsync(DateOnly date, string site, Session[] sessions, ApplyAvailabilityMode mode,
        Session sessionToEdit = null)
    {
        if (string.IsNullOrEmpty(site))
        {
            throw new ArgumentException("Site must have a value.");
        }

        if (sessions is null || sessions.Length == 0)
        {
            throw new ArgumentException("Availability must contain one or more sessions.");
        }

        if (mode is ApplyAvailabilityMode.Edit && sessionToEdit is null)
        {
            throw new ArgumentException("When editing a session a session to edit must be supplied.");
        }

        await availabilityStore.ApplyAvailabilityTemplate(site, date, sessions, mode, sessionToEdit);
        await bookingWriteService.RecalculateAppointmentStatuses(site, date);
    }

    public async Task CancelSession(string site, DateOnly date, string from, string until, string[] services, int slotLength, int capacity)
    {
        var sessionToCancel = new Session
        {
            Capacity = capacity,
            From = TimeOnly.Parse(from, CultureInfo.InvariantCulture),
            Until = TimeOnly.Parse(until, CultureInfo.InvariantCulture),
            Services = services,
            SlotLength = slotLength,
        };

        await availabilityStore.CancelSession(site, date, sessionToCancel);
        await bookingWriteService.RecalculateAppointmentStatuses(site, date);
    }

    public async Task<(int cancelledBookingsCount, int bookingsWithoutContactDetailsCount)> CancelDayAsync(string site, DateOnly date)
    {
        var cancelDayTask = availabilityStore.CancelDayAsync(site, date);
        var cancelBookingsTask = bookingWriteService.CancelAllBookingsInDayAsync(site, date);

        await Task.WhenAll(cancelDayTask, cancelBookingsTask);

        var (cancelledBookingCount, bookingsWithoutContactDetailsCount) = cancelBookingsTask.Result;

        return (cancelledBookingCount, bookingsWithoutContactDetailsCount);
    }

    private static IEnumerable<DateOnly> GetDatesBetween(DateOnly start, DateOnly end, params DayOfWeek[] weekdays)
    {
        var cursor = start;
        while (cursor <= end)
        {
            if (weekdays.Contains(cursor.DayOfWeek))
                yield return cursor;
            cursor = cursor.AddDays(1);
        }
    }

    public async Task<SessionModificationResult> EditOrCancelSessionAsync(
        string site, 
        DateOnly from, 
        DateOnly until, 
        Session sessionMatcher, 
        Session sessionReplacement,
        NewlyUnsupportedBookingAction newlyUnsupportedBookingAction = NewlyUnsupportedBookingAction.Orphan
    )
    {
        var multipleDays = from != until;
        var hasReplacement = sessionReplacement is not null;

        var updateAction = DetermineSessionUpdateAction(multipleDays, hasReplacement);

        (bool success, string message) result;

        switch (updateAction)
        {
            case SessionUpdateAction.CancelMultiple:
                var cancelResult = await availabilityStore.CancelMultipleSessions(site, from, until, sessionMatcher!);
                result = (cancelResult.Success, cancelResult.Message);
                break;
            case SessionUpdateAction.EditMultiple:
                var editResult = await availabilityStore.EditSessionsAsync(site, from, until, sessionMatcher, sessionReplacement);
                result = (editResult.Success, editResult.Message);
                break;
            case SessionUpdateAction.EditSingle:
                await availabilityStore.ApplyAvailabilityTemplate(
                    site,
                    from,
                    [sessionReplacement],
                    ApplyAvailabilityMode.Edit,
                    sessionMatcher!);

                result = (true, string.Empty);
                break;
            case SessionUpdateAction.CancelSingle:
            default:
                await availabilityStore.CancelSession(site, from, sessionMatcher);
                result = (true, string.Empty);
                break;
        }

        if (!result.success)
        {
            return new SessionModificationResult(false, result.message);
        }

        var days = Enumerable.Range(0, until.DayNumber - from.DayNumber + 1).Select(x => from.AddDays(x)).ToArray();
        var stats = await bookingWriteService.RecalculateAppointmentStatuses(site, days, newlyUnsupportedBookingAction);

        return new SessionModificationResult(true, result.message, stats.BookingsCanceled, stats.BookingsCanceledWithoutDetails);
    }

    public async Task<(int cancelledSessionsCount, int cancelledBookingsCount, int bookingsWithoutContactDetailsCount)> CancelDateRangeAsync(
        string site, DateOnly from, DateOnly until, bool cancelBookings, bool cancelDateRangeWithBookingsEnabled)
    {
        using var leaseContent = siteLeaseManager.Acquire(site);

        var cancelledBookingsCount = 0;
        var bookingsWithoutContactDetailsCount = 0;
        var cancelledSessionsCount = await availabilityStore.CancelAllSessionsInDateRange(site, from, until);

        if (cancelBookings && cancelDateRangeWithBookingsEnabled)
        {
            var result = await bookingWriteService.CancelAllBookingsInDateRangeAsync(site, from, until);
            cancelledBookingsCount = result.cancelledBookingsCount;
            bookingsWithoutContactDetailsCount = result.bookingsWithoutContactDetailsCount;
        }

        return (cancelledSessionsCount, cancelledBookingsCount, bookingsWithoutContactDetailsCount);
    }

    private static SessionUpdateAction DetermineSessionUpdateAction(bool isMultipleDays, bool hasReplacement)
    {
        if (!hasReplacement && isMultipleDays)
        {
            return SessionUpdateAction.CancelMultiple;
        }

        if (hasReplacement && isMultipleDays)
        {
            return SessionUpdateAction.EditMultiple;
        }

        return hasReplacement ? SessionUpdateAction.EditSingle : SessionUpdateAction.CancelSingle;
    }
}
