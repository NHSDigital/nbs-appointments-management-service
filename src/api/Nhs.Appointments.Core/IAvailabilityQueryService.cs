namespace Nhs.Appointments.Core;

public interface IAvailabilityQueryService
{
    Task<IEnumerable<AvailabilityCreatedEvent>> GetAvailabilityCreatedEventsAsync(string site, DateOnly from);
    Task<IEnumerable<DailyAvailability>> GetDailyAvailability(string site, DateOnly from, DateOnly to);
    Task<IEnumerable<LinkedSessionInstance>> GetLinkedSessions(string site, DateOnly from, DateOnly to, bool generateInternalSessionId = false);
    
    Task<IEnumerable<SessionInstance>> GetSessionsForServiceDescending(string site, string service, DateOnly from, DateOnly to);
}
