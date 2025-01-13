namespace Nhs.Appointments.Core;

public interface IAvailabilityStore
{
    Task<IEnumerable<SessionInstance>> GetSessions(string site, DateOnly notBefore, DateOnly notAfter);
   Task ApplyAvailabilityTemplate(string site, DateOnly date, Session[] sessions, ApplyAvailabilityMode mode,
        Session sessionToEdit = null);
    Task<IEnumerable<DailyAvailability>> GetDailyAvailability(string site, DateOnly from, DateOnly to);
    Task<SessionInstance> CancelSession(string site, DateOnly date, Session session);
}
