namespace Nhs.Appointments.Core;

public interface IAvailabilityStore
{
    Task<IEnumerable<SessionInstance>> GetSessions(string site, DateOnly notBefore, DateOnly notAfter);
    Task ApplyTemplate(string site, DateOnly date, Session[] sessions);
}