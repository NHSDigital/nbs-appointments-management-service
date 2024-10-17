namespace Nhs.Appointments.Core;

public interface IScheduleService
{
    Task<IEnumerable<SessionInstance>> GetSessions(string site, string service, DateOnly from, DateOnly until);    
}

public class ScheduleService(IAvailabilityDocumentStore availabilityDocumentStore): IScheduleService
{   
    public async Task<IEnumerable<SessionInstance>> GetSessions(string site, string service, DateOnly from, DateOnly until)
    {
        var sessions = await availabilityDocumentStore.GetSessions(site, from, until);
        return sessions.Where(s => s.Services.Contains(service));
    }
}        
