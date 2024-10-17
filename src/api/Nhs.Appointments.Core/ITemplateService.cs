namespace Nhs.Appointments.Core;

public interface IAvailabilityDocumentStore
{
    Task<IEnumerable<SessionInstance>> GetSessions(string site, DateOnly notBefore, DateOnly notAfter);
}