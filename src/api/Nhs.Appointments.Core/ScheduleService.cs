using Nhs.Appointments.Core.Extensions;

namespace Nhs.Appointments.Core;

public interface IScheduleService
{
    Task<IEnumerable<SessionInstance>> GetSessions(string site, string service, DateOnly from, DateOnly until);
    Task UpdateDisabledServiceTypes(string siteId, IEnumerable<string> disableServiceTypes);
}

public class ScheduleService: IScheduleService
{
    private readonly ITemplateDocumentStore _templateDocumentStore;
    public ScheduleService(ITemplateDocumentStore templateDocumentStore) 
    { 
        _templateDocumentStore = templateDocumentStore;
    }
    
    public async Task<IEnumerable<SessionInstance>> GetSessions(string site, string service, DateOnly from, DateOnly until)
    {
        var sessions = new List<SessionInstance>();
        var templates = await _templateDocumentStore.GetTemplates(site);
        var assignments = await _templateDocumentStore.GetTemplateAssignmentsAsync(site);        
        
        var day = from;
        while (day <= until)
        {
            var assignment = assignments.FirstOrDefault(a => a.From <= day && a.Until >= day); // TODO: Have a robust mechanism for cascading
            if (assignment != null)
            {
                var template = templates.Single(t => t.Id == assignment.TemplateId);
                var scheduleBlocks = template.Items.SingleOrDefault(i => i.Days.Contains(day.DayOfWeek))?.ScheduleBlocks ?? Enumerable.Empty<ScheduleBlock>();
                sessions.AddRange(
                    scheduleBlocks.Where(s => s.Services.Contains(service))
                    .Select(s => new SessionInstance(day.ToDateTime(s.From), day.ToDateTime(s.Until))
                    {
                        SessionHolder = "default"
                    }) ?? new SessionInstance[0]);
            }
            
            day = day.AddDays(1);
        }
        
                                
        return sessions;
    }

    public async Task UpdateDisabledServiceTypes(string siteId, IEnumerable<string> disableServiceTypes)
    {        
        var templates = await _templateDocumentStore.GetTemplates(siteId);
        foreach(var template in templates) 
        {
            template.RemoveServices(disableServiceTypes);
            await _templateDocumentStore.SaveTemplateAsync(template);
        }        
    }
}        
