using Nhs.Appointments.Core.Concurrency;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;

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
}
