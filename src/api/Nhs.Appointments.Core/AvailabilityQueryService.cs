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
    
    public async Task<IEnumerable<SessionInstance>> GetSlots(string site, DateOnly from, DateOnly to)
    {
        var sessionsOnThatDay = await availabilityStore.GetSessions(site, from, to);
        return sessionsOnThatDay.SelectMany(session => session.ToSlots());
    }
}
