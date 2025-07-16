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
            RemainingCapacity = summary.RemainingCapacity,
            MaximumCapacity = summary.MaximumCapacity,
            GeneratedAtUtc = summary.GeneratedAtUtc,
            DocumentType = store.GetDocumentType()
        });
    }

    public async Task<IEnumerable<DailySiteSummary>> GetSiteSummarys(string site, DateOnly from, DateOnly to)
    {
        return await store.RunQueryAsync<DailySiteSummary>(x => x.Id == site && x.Date >= from && x.Date <= to);
    }
    
    public async Task IfExistsDelete(string site, DateOnly date)
    {
        await store.DeleteDocument(site, date.ToString("yyyy-MM-dd"));
    }
}
