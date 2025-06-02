namespace Nhs.Appointments.Core;

public interface IAvailabilityQueryService
{
    Task<IEnumerable<AvailabilityCreatedEvent>> GetAvailabilityCreatedEventsAsync(string site, DateOnly from);
    Task<IEnumerable<DailyAvailability>> GetDailyAvailability(string site, DateOnly from, DateOnly to);

    // Task<IEnumerable<SessionInstance>> GetSlots(string site, DateOnly from, DateOnly to, bool generateSessionId = false);
    Task<IEnumerable<SessionInstance>> GetSessions(string site, DateOnly from, DateOnly to, bool generateSessionId = false);
}
