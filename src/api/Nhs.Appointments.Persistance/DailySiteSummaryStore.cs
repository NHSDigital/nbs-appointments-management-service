using Nhs.Appointments.Core.Reports.SiteSummary;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class DailySiteSummaryStore(ITypedDocumentCosmosStore<DailySiteSummaryDocument> store) : IDailySiteSummaryStore
{
    public async Task CreateDailySiteSummary(DailySiteSummary summary)
    {
        await store.WriteAsync(new DailySiteSummaryDocument()
        {
            Id = summary.Site,
            Date = summary.Date,
            Bookings = summary.Bookings,
            Orphaned = summary.Orphaned,
            Cancelled = summary.Cancelled,
            RemainingCapacity = summary.RemainingCapacity,
            MaximumCapacity = summary.MaximumCapacity,
            GeneratedAtUtc = summary.GeneratedAtUtc,
            DocumentType = store.GetDocumentType()
        });
    }

    public async Task<IEnumerable<DailySiteSummary>> GetSiteSummaries(string site, DateOnly from, DateOnly to)
    {
        var summaries = await store.RunQueryAsync(x => x.Id == site && x.Date >= from && x.Date <= to);
        return summaries.Select(MapToDailySiteSummary);
    }
    
    public async Task IfExistsDelete(string site, DateOnly date)
    {
        var document = await store.GetByIdOrDefaultAsync(site, date.ToString("yyyy-MM-dd"));

        if (document is not null)
        {
            await store.DeleteDocument(site, date.ToString("yyyy-MM-dd"));
        }
    }

    private static DailySiteSummary MapToDailySiteSummary(DailySiteSummaryDocument document)
    {
        return document is null ? null : new DailySiteSummary()
        {
            Site = document.Id,
            Date = document.Date,
            Bookings = document.Bookings,
            Orphaned = document.Orphaned,
            Cancelled = document.Cancelled,
            RemainingCapacity = document.RemainingCapacity,
            MaximumCapacity = document.MaximumCapacity,
            GeneratedAtUtc = document.GeneratedAtUtc
        };
    }
}
