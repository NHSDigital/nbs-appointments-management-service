using System.Linq.Expressions;
using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class AvailabilityDocumentStore(
    ITypedDocumentCosmosStore<DailyAvailabilityDocument> documentStore,
    IMetricsRecorder metricsRecorder) : IAvailabilityStore
{
    public async Task<IEnumerable<SessionInstance>> GetSessions(string site, DateOnly from, DateOnly to)
    {
        var results = new List<SessionInstance>();
        var docType = documentStore.GetDocumentType();
        using (metricsRecorder.BeginScope("GetSessions"))
        {
            var documents = await documentStore.RunQueryAsync<DailyAvailabilityDocument>(b =>
                b.DocumentType == docType && b.Site == site && b.Date >= from && b.Date <= to);

            foreach (var day in documents)
            {
                results.AddRange(day.Sessions.Select(s =>
                    new SessionInstance(day.Date.ToDateTime(s.From), day.Date.ToDateTime(s.Until))
                    {
                        Services = s.Services, SlotLength = s.SlotLength, Capacity = s.Capacity
                    }
                ));
            }
        }

        return results;
    }

    public async Task ApplyAvailabilityTemplate(string site, DateOnly date, Session[] sessions,
        ApplyAvailabilityMode mode, Session sessionToEdit = null)
    {
        var documentType = documentStore.GetDocumentType();
        var documentId = date.ToString("yyyyMMdd");

        switch (mode)
        {
            case ApplyAvailabilityMode.Overwrite:
                await WriteDocument(date, documentType, documentId, sessions, site);
                break;
            case ApplyAvailabilityMode.Additive:
                var originalDocument = await GetOrDefaultAsync(documentId, site);
                if (originalDocument == null)
                {
                    await WriteDocument(date, documentType, documentId, sessions, site);
                }
                else
                {
                    await PatchAvailabilityDocument(documentId, sessions, site, originalDocument);
                }

                break;
            case ApplyAvailabilityMode.Edit:
                await EditExistingSession(documentId, site, sessions.Single(), sessionToEdit);
                break;
            default:
                throw new NotSupportedException();
        }
    }

    public async Task<IEnumerable<DailyAvailability>> GetDailyAvailability(string site, DateOnly from, DateOnly to)
    {
        var docType = documentStore.GetDocumentType();
        return await documentStore.RunQueryAsync<DailyAvailability>(b =>
            b.DocumentType == docType && b.Site == site && b.Date >= from && b.Date <= to);
    }

    public async Task<SessionInstance> CancelSession(string site, DateOnly date, Session session)
    {
        var documentId = date.ToString("yyyyMMdd");
        var dayDocument = await GetOrDefaultAsync(documentId, site);
        var extantSessions = dayDocument.Sessions.ToList();

        var firstMatchingSession = FindMatchingSession(extantSessions, session);
        if (firstMatchingSession is null)
        {
            throw new InvalidOperationException(
                "The requested Session to cancel could not be found in the sessions for that day.");
        }

        extantSessions.Remove(firstMatchingSession);

        await PatchAvailabilityDocument(documentId, [.. extantSessions], site, dayDocument, false);

        return new SessionInstance(dayDocument.Date.ToDateTime(session.From),
            dayDocument.Date.ToDateTime(session.Until))
        {
            Services = session.Services, SlotLength = session.SlotLength, Capacity = session.Capacity,
        };
    }

    public async Task<bool> SiteOffersServiceDuringPeriod(string siteId, string service, List<string> datesInPeriod)
    {
        using (metricsRecorder.BeginScope("SiteOffersServiceDuringPeriod"))
        {
            var docType = documentStore.GetDocumentType();
            
            var query = new QueryDefinition(
                    query: "SELECT VALUE COUNT(1) " +
                           "FROM booking_data bd " +
                           "JOIN s IN bd.sessions " +
                           "WHERE ARRAY_CONTAINS(@docIds, bd.id) AND bd.site = @site AND bd.docType = @docType AND ARRAY_CONTAINS(s.services, @service)")
                .WithParameter("@docType", docType)
                .WithParameter("@docIds", datesInPeriod)
                .WithParameter("@site", siteId)
                .WithParameter("@service", service);
        
            var dailyAvailabilityCount = (await documentStore.RunSqlQueryAsync<int>(query)).Single();
            return dailyAvailabilityCount > 0;
        }
    }

    public async Task CancelDayAsync(string site, DateOnly date)
    {
        var documentId = date.ToString("yyyyMMdd");
        var document = await GetOrDefaultAsync(documentId, site)
            ?? throw new InvalidOperationException($"The requested Availability for site {site} could not be found on date: {date}.");

        await documentStore.DeleteDocument(documentId, site);
    }

    private async Task EditExistingSession(string documentId, string site, Session newSession, Session sessionToEdit)
    {
        var dayDocument = await GetOrDefaultAsync(documentId, site);
        var extantSessions = dayDocument.Sessions.ToList();

        var firstMatchingSession = FindMatchingSession(extantSessions, sessionToEdit);
        if (firstMatchingSession is null)
        {
            throw new InvalidOperationException(
                "The requested Session to edit could not be found in the sessions for that day.");
        }

        extantSessions.Remove(firstMatchingSession);

        var patchedSessions = extantSessions
            .Append(newSession)
            .ToArray();

        await PatchAvailabilityDocument(documentId, patchedSessions, site, dayDocument, false);
    }

    private async Task WriteDocument(DateOnly date, string documentType, string documentId, Session[] sessions,
        string site)
    {
        var document = new DailyAvailabilityDocument
        {
            Date = date,
            DocumentType = documentType,
            Id = documentId,
            Sessions = sessions,
            Site = site
        };
        await documentStore.WriteAsync(document);
    }

    private async Task PatchAvailabilityDocument(string documentId, Session[] sessions, string site,
        DailyAvailabilityDocument originalDocument, bool concatSessions = true)
    {
        if (concatSessions)
        {
            var originalSessions = originalDocument.Sessions;
            var newSessions = originalSessions.Concat(sessions);
            var dailyAvailabilityDocumentPatch = PatchOperation.Add("/sessions", newSessions);
            await documentStore.PatchDocument(site, documentId, dailyAvailabilityDocumentPatch);
        }
        else
        {
            var dailyAvailabilityDocumentPatch = PatchOperation.Add("/sessions", sessions);
            await documentStore.PatchDocument(site, documentId, dailyAvailabilityDocumentPatch);
        }
    }

    private async Task<DailyAvailabilityDocument> GetOrDefaultAsync(string documentId, string partitionKey)
    {
        return await documentStore.GetByIdOrDefaultAsync<DailyAvailabilityDocument>(documentId, partitionKey);
    }

    private static Session FindMatchingSession(List<Session> sessions, Session sessionToMatch)
    {
        return sessions.FirstOrDefault(session =>
            session.From == sessionToMatch.From
            && session.Until == sessionToMatch.Until
            && session.Services.SequenceEqual(sessionToMatch.Services)
            && session.SlotLength == sessionToMatch.SlotLength
            && session.Capacity == sessionToMatch.Capacity);
    }
}
