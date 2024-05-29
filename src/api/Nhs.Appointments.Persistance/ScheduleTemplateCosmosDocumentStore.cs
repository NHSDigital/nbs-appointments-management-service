using Nhs.Appointments.Persistance.Models;
using Nhs.Appointments.Core;
using Microsoft.Azure.Cosmos;

namespace Nhs.Appointments.Persistance;
public class WeekTemplateCosmosDocumentStore : ITemplateDocumentStore
{
    private readonly ITypedDocumentCosmosStore<WeekTemplateDocument> _templateStore;
    private readonly ITypedDocumentCosmosStore<TemplateAssignmentDocument> _assignmentStore;

    public WeekTemplateCosmosDocumentStore(
        ITypedDocumentCosmosStore<WeekTemplateDocument> templateStore,
        ITypedDocumentCosmosStore<TemplateAssignmentDocument> assignmentStore)
    {
        _templateStore = templateStore;
        _assignmentStore = assignmentStore;
    }

    public Task<IEnumerable<WeekTemplate>> GetTemplates(string site)
    {
        var docType = _templateStore.GetDocumentType();
        return _templateStore.RunQueryAsync<WeekTemplate>(sc => sc.Site == site && sc.DocumentType == docType);
    }

    public async Task<string> SaveTemplateAsync(WeekTemplate template)
    {
        var document = _templateStore.ConvertToDocument(template);
        if(string.IsNullOrEmpty(document.Id))
            document.Id = Guid.NewGuid().ToString();
        await _templateStore.WriteAsync(document);
        return document.Id;
    }

    public Task SaveTemplateAssignmentsAsync(string site, IEnumerable<TemplateAssignment> assignments)
    {
        var document = _assignmentStore.NewDocument();
        document.Site = site;
        document.Id = "default";
        document.Assignments = assignments.ToArray();
        return _assignmentStore.WriteAsync(document);
    }

    public async Task<IEnumerable<TemplateAssignment>> GetTemplateAssignmentsAsync(string site)
    {
        try
        {
            var assignmentDoc = await _assignmentStore.GetDocument<TemplateAssignmentDocument>("default", site);
            return assignmentDoc.Assignments;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Enumerable.Empty<TemplateAssignment>();
        }
    }
}