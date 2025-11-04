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

        var wellKnownOdsCodes = await wellKnownOdsCodesStore.GetWellKnownOdsCodesDocument();

        foreach (var site in sites)
        {
            report.Add(await Generate(site, clinicalServices, wellKnownOdsCodes, startDate, endDate));
        }

        return report
            .OrderBy(x => x.Region)
            .ThenBy(x => x.ICB)
            .ThenBy(x => x.OdsCode)
            .ThenBy(x => x.SiteName);
    }

    private async Task<SiteReport> Generate(Site site, string[] clinicalServices,
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
