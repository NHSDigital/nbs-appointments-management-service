namespace Nhs.Appointments.Core;

public class AvailabilityQueryService(
    IAvailabilityStore availabilityStore,
    IAvailabilityCreatedEventStore availabilityCreatedEventStore) : IAvailabilityQueryService
{
    public async Task<IEnumerable<AvailabilityCreatedEvent>> GetAvailabilityCreatedEventsAsync(string site, DateOnly from)
    {
        var events = await availabilityCreatedEventStore.GetAvailabilityCreatedEvents(site);

        return events
            .Where(acEvent => (acEvent.To ?? acEvent.From) >= from)
            .OrderBy(e => e.From)
            .ThenBy(e => e.To);
    }
    
    public async Task<IEnumerable<DailyAvailability>> GetDailyAvailability(string site, DateOnly from, DateOnly to)
    {
        return await availabilityStore.GetDailyAvailability(site, from, to);
    }
    
    public async Task<IEnumerable<LinkedSessionInstance>> GetLinkedSessions(string site, DateOnly from, DateOnly to, bool generateInternalSessionId = false)
    {
        var sessions = await availabilityStore.GetSessions(site, from, to);
        return sessions.Select(x => new LinkedSessionInstance(x, generateInternalSessionId));
    }

    /// <summary>
    /// Fetch all sessions that support the service, and order them 'from' descending
    /// </summary>
    public async Task<IEnumerable<SessionInstance>> GetSessionsForServiceDescending(string site, string service, DateOnly from, DateOnly to)
    {
        return (await availabilityStore.GetSessionsForService(site, service, from, to)).OrderByDescending(x => x.From);
    }
}
