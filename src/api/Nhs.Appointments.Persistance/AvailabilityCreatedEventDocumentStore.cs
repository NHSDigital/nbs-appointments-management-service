using Nhs.Appointments.Persistance.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Persistance;

public class AvailabilityCreatedEventDocumentStore(ITypedDocumentCosmosStore<AvailabilityCreatedEventDocument> documentStore, TimeProvider time) : IAvailabilityCreatedEventStore
{
    public async Task LogTemplateCreated(string site, DateOnly from, DateOnly until, Template template, string user)
    {
        var documentType = documentStore.GetDocumentType();
        var timeStamp = time.GetUtcNow().UtcDateTime;
        var documentId = $"availability_created_{site}_{timeStamp:yyyyMMddHHmmss}";

        var document = new AvailabilityCreatedEventDocument()
        {
            Id = documentId,
            DocumentType = documentType,
            Created = time.GetUtcNow().UtcDateTime,
            By = user,
            Site = site,
            Template = template,
            From = from,
            To = until,
        };

        await documentStore.WriteAsync(document);
    }

    public async Task LogSingleDateSessionCreated(string site, DateOnly date, Session[] sessions, string user)
    {
        var documentType = documentStore.GetDocumentType();
        var timeStamp = time.GetUtcNow().UtcDateTime;
        var documentId = $"availability_created_{site}_{timeStamp:yyyyMMddHHmmss}";

        var document = new AvailabilityCreatedEventDocument()
        {
            Id = documentId,
            DocumentType = documentType,
            Created = time.GetUtcNow().UtcDateTime,
            By = user,
            Site = site,
            Sessions = sessions,
            From = date
        };

        await documentStore.WriteAsync(document);
    }

    public async Task<IEnumerable<AvailabilityCreatedEvent>> GetAvailabilityCreatedEvents(string site)
    {
        var docType = documentStore.GetDocumentType();

        var documents = await documentStore.RunQueryAsync<AvailabilityCreatedEventDocument>(b => b.DocumentType == docType && b.Site == site);

        return documents.Select(document => new AvailabilityCreatedEvent()
        {
            By = document.By,
            Created = document.Created,
            Site = document.Site,
            Sessions = document.Sessions,
            Template = document.Template,
            From = document.From,
            To = document.To,
        });
    }
}