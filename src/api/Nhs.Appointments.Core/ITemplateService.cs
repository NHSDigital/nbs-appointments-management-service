namespace Nhs.Appointments.Core;

public interface ITemplateService
{
    Task<string> SaveTemplate(WeekTemplate template);
    Task<IEnumerable<WeekTemplate>> GetTemplates(string site);
    Task SaveAssignmentsAsync(string site, IEnumerable<TemplateAssignment> templates);
    Task<IEnumerable<TemplateAssignment>> GetAssignmentsAsync(string site);
}

public class TemplateService : ITemplateService
{
    private readonly ITemplateDocumentStore _templateDocumentStore;

    public TemplateService(ITemplateDocumentStore templateDocumentStore) 
    { 
        _templateDocumentStore = templateDocumentStore;
    }

    public Task<IEnumerable<WeekTemplate>> GetTemplates(string site)
    {
        return _templateDocumentStore.GetTemplates(site);
    }

    public Task<string> SaveTemplate(WeekTemplate template)
    {
        return _templateDocumentStore.SaveTemplateAsync(template);
    }

    public Task SaveAssignmentsAsync(string site, IEnumerable<TemplateAssignment> assignments)
    {
        return _templateDocumentStore.SaveTemplateAssignmentsAsync(site, assignments);
    }

    public Task<IEnumerable<TemplateAssignment>> GetAssignmentsAsync(string site)
    {
        return _templateDocumentStore.GetTemplateAssignmentsAsync(site);
    }
}

public interface ITemplateDocumentStore
{
    Task<string> SaveTemplateAsync(WeekTemplate template);
    Task<IEnumerable<WeekTemplate>> GetTemplates(string site);
    Task SaveTemplateAssignmentsAsync(string site, IEnumerable<TemplateAssignment> assignments);
    Task<IEnumerable<TemplateAssignment>> GetTemplateAssignmentsAsync(string site);
}
