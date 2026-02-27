namespace Nhs.Appointments.Core.Availability;

public interface IAvailabilityStore
{
    Task<IEnumerable<SessionInstance>> GetSessions(string site, DateOnly from, DateOnly to);
    Task ApplyAvailabilityTemplate(string site, DateOnly date, Session[] sessions, ApplyAvailabilityMode mode,
        Session sessionToEdit = null);
   Task<IEnumerable<DailyAvailability>> GetDailyAvailability(string site, DateOnly from, DateOnly to);
   Task<SessionInstance> CancelSession(string site, DateOnly date, Session session);
   Task<bool> SiteSupportsAllServicesOnSingleDateInRangeAsync(string siteId, List<string> services, List<string> datesInPeriod);
    Task CancelDayAsync(string site, DateOnly date);
    Task<OperationResult> EditSessionsAsync(string site, DateOnly from, DateOnly until, Session sessionMatcher, Session sessionReplacement);
    Task<OperationResult> CancelMultipleSessions(string site, DateOnly from, DateOnly until, Session sessionMatcher = null);
    Task<int> CancelAllSessionsInDateRange(string site, DateOnly from, DateOnly until);
}
