namespace Nhs.Appointments.Core;

public class AvailabilityService(IAvailabilityStore availabilityStore, IAvailabilityCreatedEventStore availabilityCreatedEventStore) : IAvailabilityService
{
    public async Task ApplyAvailabilityTemplateAsync(string site, DateOnly from, DateOnly until, Template template, string user)
    {
        if (string.IsNullOrEmpty(site))
            throw new ArgumentException("site must have a value");

        if (from > until)
            throw new ArgumentException("until date must be after from date");

        if (template == null)
            throw new ArgumentException("template must be provided");

        if (template.Sessions is null || template.Sessions.Length == 0)
            throw new ArgumentException("template must contain one or more sessions");

        if (template.Days is null || template.Days.Length == 0)
            throw new ArgumentException("template must specify one or more weekdays");

        var dates = GetDatesBetween(from, until, template.Days);
        foreach (var date in dates)
        {
            await availabilityStore.ApplyAvailabilityTemplate(site, date, template.Sessions);
        }

        await availabilityCreatedEventStore.LogTemplateCreated(site, from, until, template, user);
    }

    public async Task ApplySingleDateSessionAsync(DateOnly date, string site, Session[] sessions, string user)
    {
        await SetAvailabilityAsync(date, site, sessions);
        await availabilityCreatedEventStore.LogSingleDateSessionCreated(site, date, sessions, user);
    }

    public async Task<IEnumerable<AvailabilityCreatedEvent>> GetAvailabilityCreatedEventsAsync(string site)
    {
        var events = await availabilityCreatedEventStore.GetAvailabilityCreatedEvents(site);

        return events.OrderBy(e => e.From).ThenBy(e => e.To);
    }

    public async Task SetAvailabilityAsync(DateOnly date, string site, Session[] sessions)
    {
        if (string.IsNullOrEmpty(site))
            throw new ArgumentException("Site must have a value.");

        if (sessions is null || sessions.Length == 0)
            throw new ArgumentException("Availability must contain one or more sessions.");

        await availabilityStore.ApplyAvailabilityTemplate(site, date, sessions);
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
}
