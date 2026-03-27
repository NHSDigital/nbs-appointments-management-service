using Nhs.Appointments.Core.Reports.SiteSummary;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class DailySiteSummaryStore(
    ITypedDocumentCosmosStore<DailySiteSummaryDocument> store,
    IMetricsRecorder metricsRecorder
    ) : IDailySiteSummaryStore
{
    public async Task CreateDailySiteSummary(DailySiteSummary summary)
    {
        using (metricsRecorder.BeginScope(MetricScopes.DailySiteSummary.Create))
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
    }

    public async Task<IEnumerable<DailySiteSummary>> GetSiteSummaries(string site, DateOnly from, DateOnly to)
    {
        using (metricsRecorder.BeginScope(MetricScopes.DailySiteSummary.GetSiteSummaries))
        {
            return await store.RunQueryAsync<DailySiteSummary>(x => x.Id == site && x.Date >= from && x.Date <= to);
        }
    }

    public async Task IfExistsDelete(string site, DateOnly date)
    {
        using (metricsRecorder.BeginScope(MetricScopes.DailySiteSummary.DeleteSummary))
        {
            var document = await store.GetByIdOrDefaultAsync<DailySiteSummaryDocument>(site, date.ToString("yyyy-MM-dd"));

            if (document is not null)
            {
                await store.DeleteDocument(site, date.ToString("yyyy-MM-dd"));
            }
        }
    }
}
