using Microsoft.Extensions.Options;
using Nhs.Appointments.Core.ClinicalServices;
using Nhs.Appointments.Core.OdsCodes;
using Nhs.Appointments.Core.Reports.SiteSummary;

namespace Nhs.Appointments.Core.Sites;

public class SiteReportService(
    IDailySiteSummaryStore dailySiteSummaryStore,
    IClinicalServiceStore clinicalServiceStore,
    IWellKnownOdsCodesStore wellKnownOdsCodesStore,
    IOptions<SiteSummaryQueryOptions> siteSummaryQueryOptions) : ISiteReportService
{
    private readonly int _parallelizationMinimum = siteSummaryQueryOptions.Value.MinimumParallelization;
    public async Task<IEnumerable<SiteReport>> GenerateReports(IEnumerable<Site> sites, DateOnly startDate,
        DateOnly endDate)
    {
        var clinicalServices = (await clinicalServiceStore.Get())
            .Select(x => x.Value)
            .ToArray();

        var wellKnownOdsCodes = (await wellKnownOdsCodesStore.GetWellKnownOdsCodesDocument()).ToList();

        var report = await GenerateReport(sites.ToArray(), clinicalServices, wellKnownOdsCodes, startDate, endDate);

        return report
            .OrderBy(x => x.Region)
            .ThenBy(x => x.ICB)
            .ThenBy(x => x.OdsCode)
            .ThenBy(x => x.SiteName);
    }

    private async Task<IEnumerable<SiteReport>> GenerateReport(Site[] sites, string[] clinicalServices,
        IEnumerable<WellKnownOdsEntry> wellKnownOdsCodes, DateOnly startDate,
        DateOnly endDate)
    {
        if (sites.Length == 0)
        {
            return Enumerable.Empty<SiteReport>();
        }

        if (sites.Length < _parallelizationMinimum)
        {
            return await Task.WhenAll(sites.Select(site =>
                GenerateReport(site, clinicalServices, wellKnownOdsCodes, startDate, endDate)));
        }
        var report =  new List<SiteReport>();

        await Parallel.ForEachAsync(sites, async (site, _) =>
        {
            report.Add(await GenerateReport(site, clinicalServices, wellKnownOdsCodes, startDate, endDate));
        });
            
        return report;
    }

    private async Task<SiteReport> GenerateReport(Site site, string[] clinicalServices,
        IEnumerable<WellKnownOdsEntry> wellKnownOdsCodes, DateOnly startDate,
        DateOnly endDate)
    {
        var siteReport = await dailySiteSummaryStore.GetSiteSummaries(
            site.Id, 
            startDate, 
            endDate);
        return new SiteReport(site, siteReport.ToArray(), clinicalServices, wellKnownOdsCodes);
    }
}
