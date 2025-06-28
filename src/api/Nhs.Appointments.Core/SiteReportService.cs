namespace Nhs.Appointments.Core;

public class SiteReportService(
    IBookingAvailabilityStateService bookingAvailabilityStateService,
    IClinicalServiceStore clinicalServiceStore) : ISiteReportService
{
    public async Task<IEnumerable<SiteReport>> Generate(Site[] sites, DateOnly startDate, DateOnly endDate)
    {
        var report = new List<SiteReport>();
        var clinicalServices = (await clinicalServiceStore.Get())
            .Select(x => x.Value)
            .ToArray();
        
        foreach (var site in sites)
        {
            report.Add(await Generate(site, clinicalServices, startDate, endDate));
        }

        return report
            .OrderBy(x => x.Region)
            .ThenBy(x => x.ICB)
            .ThenBy(x => x.OdsCode)
            .ThenBy(x => x.SiteName);
    }

    private async Task<SiteReport> Generate(Site site, string[] clinicalServices, DateOnly startDate, DateOnly endDate)
    {
        var siteReport = await bookingAvailabilityStateService.GetSummaries(
            site.Id, 
            startDate.ToDateTime(new TimeOnly(0, 0)), 
            endDate.ToDateTime(new TimeOnly(23, 59, 59)));
        return new SiteReport(site, siteReport.ToArray(), clinicalServices);
    }
}
