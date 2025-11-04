using Nhs.Appointments.Core.Reports.SiteSummary;

namespace Nhs.Appointments.Core;

public class SiteReportService(
    IDailySiteSummaryStore dailySiteSummaryStore,
    IClinicalServiceStore clinicalServiceStore,
    IWellKnownOdsCodesStore wellKnownOdsCodesStore) : ISiteReportService
{
    public async Task<IEnumerable<SiteReport>> Generate(Site[] sites, DateOnly startDate, DateOnly endDate)
    {
        var report = new List<SiteReport>();
        var clinicalServices = (await clinicalServiceStore.Get())
            .Select(x => x.Value)
            .ToArray();

        var wellKnownOdsCodes = (await wellKnownOdsCodesStore.GetWellKnownOdsCodesDocument()).ToList();
        var regions = wellKnownOdsCodes.Where(code => code.Type == "region").ToList();
        var icbs = wellKnownOdsCodes.Where(code => code.Type == "icb").ToList();

        foreach (var site in sites)
        {
            report.Add(await Generate(site, clinicalServices, regions, icbs, startDate, endDate));
        }

        return report
            .OrderBy(x => x.Region)
            .ThenBy(x => x.ICB)
            .ThenBy(x => x.OdsCode)
            .ThenBy(x => x.SiteName);
    }

    private async Task<SiteReport> Generate(Site site, string[] clinicalServices,
        List<WellKnownOdsEntry> regions, List<WellKnownOdsEntry> icbs, DateOnly startDate,
        DateOnly endDate)
    {
        var regionName = regions.SingleOrDefault(region => region.OdsCode == site.Region)?.DisplayName ?? "blank";
        var icbName = icbs.SingleOrDefault(icb => icb.OdsCode == site.IntegratedCareBoard)?.DisplayName ?? "blank";

        var siteReport = await dailySiteSummaryStore.GetSiteSummaries(
            site.Id, 
            startDate, 
            endDate);
        return new SiteReport(site, siteReport.ToArray(), clinicalServices, regionName, icbName);
    }
}
