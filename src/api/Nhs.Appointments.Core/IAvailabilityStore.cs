namespace Nhs.Appointments.Core;

public interface IAvailabilityStore
{
    Task<IEnumerable<SessionInstance>> GetSessions(string site, DateOnly from, DateOnly to);
    
    Task<IEnumerable<SessionInstance>> GetSessionsForService(string site, string service, DateOnly from, DateOnly to);
    
   Task ApplyAvailabilityTemplate(string site, DateOnly date, Session[] sessions, ApplyAvailabilityMode mode,
        Session sessionToEdit = null);
   Task<IEnumerable<DailyAvailability>> GetDailyAvailability(string site, DateOnly from, DateOnly to);
   Task<SessionInstance> CancelSession(string site, DateOnly date, Session session);

   Task<bool> SiteOffersServiceDuringPeriod(string siteId, string service,
       List<string> datesInPeriod);
}
