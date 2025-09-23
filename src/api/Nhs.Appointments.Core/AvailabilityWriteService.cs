using System.Globalization;

namespace Nhs.Appointments.Core;

public class AvailabilityWriteService(
    IAvailabilityStore availabilityStore,
    IAvailabilityCreatedEventStore availabilityCreatedEventStore,
    IBookingWriteService bookingWriteService) : IAvailabilityWriteService
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

    public async Task<(bool editSuccessful, string message)> EditOrCancelSessionAsync(string site, DateOnly from, DateOnly until, Session sessionMatcher, Session sessionReplacement, bool isWildcard)
    {
        var multipleDays = from != until;
        var hasReplacement = sessionReplacement is not null;

        (bool success, string message) result;

        if (isWildcard)
        {
            await CancelAllSessionInDateRange(site, from, until);
            return (true, string.Empty);
        }
        else if (!hasReplacement && multipleDays)
        {
            var cancelResult = await availabilityStore.CancelMultipleSessions(site, from, until, sessionMatcher!);
            result = (cancelResult.Success, cancelResult.Message);
        }
        else if (hasReplacement && multipleDays)
        {
            var editResult = await availabilityStore.EditSessionsAsync(site, from, until, sessionMatcher, sessionReplacement);
            result = (editResult.Success, editResult.Message);
        }
        else if (hasReplacement) // single-day replacement
        {
            await availabilityStore.ApplyAvailabilityTemplate(
                site,
                from,
                [sessionReplacement],
                ApplyAvailabilityMode.Edit,
                sessionMatcher!);

            result = (true, string.Empty);
        }
        else // single-day cancellation
        {
            await CancelDayAsync(site, from);
            result = (true, string.Empty);
        }

        if (!result.success)
        {
            return result;
        }

        var days = Enumerable.Range(0, until.DayNumber - from.DayNumber + 1)
            .Select(offset => bookingWriteService.RecalculateAppointmentStatuses(site, from.AddDays(offset)));

        await Task.WhenAll(days);

        return result;
    }

    private async Task CancelAllSessionInDateRange(string site, DateOnly from, DateOnly until)
    {
        var days = Enumerable.Range(0, until.DayNumber - from.DayNumber + 1)
                .Select(offset => CancelDayAsync(site, from.AddDays(offset)));

        await Task.WhenAll(days);
    }
}
