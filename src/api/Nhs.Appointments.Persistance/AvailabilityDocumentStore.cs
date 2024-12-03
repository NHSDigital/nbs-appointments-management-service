using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Persistance.Models;
using Nhs.Appointments.Core;
using AutoMapper;

namespace Nhs.Appointments.Persistance;

public class AvailabilityDocumentStore(ITypedDocumentCosmosStore<DailyAvailabilityDocument> documentStore, IMetricsRecorder metricsRecorder, IMapper mapper) : IAvailabilityStore
{
    public async Task<IEnumerable<SessionInstance>> GetSessions(string site, DateOnly from, DateOnly to)
    {
        var results = new List<SessionInstance>();
        var docType = documentStore.GetDocumentType();
        using (metricsRecorder.BeginScope("GetSessions"))
        {
            var documents = await documentStore.RunQueryAsync<DailyAvailabilityDocument>(b => b.DocumentType == docType && b.Site == site && b.Date >= from && b.Date <= to);
            foreach (var day in documents)
            {
                results.AddRange(day.Sessions.Select(
                    s => new SessionInstance(day.Date.ToDateTime(s.From), day.Date.ToDateTime(s.Until))
                    {
                        Services = s.Services,
                        SlotLength = s.SlotLength,
                        Capacity = s.Capacity
                    }
                    ));
            }
        }
        return results;
    }

    public async Task ApplyAvailabilityTemplate(string site, DateOnly date, Session[] sessions, ApplyAvailabilityMode mode)
    {
        var documentType = documentStore.GetDocumentType();
        var documentId = date.ToString("yyyyMMdd");
        
        switch (mode)
        {
            case (ApplyAvailabilityMode.Overwrite):
                await WriteDocument(date, documentType, documentId, sessions, site);
                break;
            case ApplyAvailabilityMode.Additive:
                var originalDocument = await GetOrDefaultAsync(documentId, site);
                if (originalDocument == null)
                    await WriteDocument(date, documentType, documentId, sessions, site);
                else 
                    await PatchAvailabilityDocument(documentId, sessions, site, originalDocument);
                break;
            default:
                throw new NotSupportedException();
        }
    }

    public async Task<IEnumerable<DailyAvailability>> GetDailyAvailability(string site, DateOnly from, DateOnly to)
    {
        var docType = documentStore.GetDocumentType();
        return await documentStore.RunQueryAsync<DailyAvailability>(b => b.DocumentType == docType && b.Site == site && b.Date >= from && b.Date <= to);
    }

    private async Task WriteDocument(DateOnly date, string documentType, string documentId, Session[] sessions, string site)
    {
        var document = new DailyAvailabilityDocument()
        {
            Date = date,
            DocumentType = documentType,
            Id = documentId,
            Sessions = sessions,
            Site = site
        };
        await documentStore.WriteAsync(document);
    }

    private async Task PatchAvailabilityDocument(string documentId, Session[] sessions, string site, DailyAvailabilityDocument originalDocument)
    {
        var originalSessions = originalDocument.Sessions;
        var newSessions = originalSessions.Concat(sessions);
        var dailyAvailabilityDocumentPatch = PatchOperation.Add("/sessions", newSessions);
        await documentStore.PatchDocument(site, documentId, dailyAvailabilityDocumentPatch);
    }
    
    private async Task<DailyAvailabilityDocument?> GetOrDefaultAsync(string documentId, string partitionKey)
    {
        return await documentStore.GetByIdOrDefaultAsync<DailyAvailabilityDocument>(documentId, partitionKey);
    }
}