using Nhs.Appointments.Core.Sites;

namespace Nhs.Appointments.Core.Reports.SiteSummary;

public class SiteReportCsvWriter(TimeProvider timeProvider) : ISiteReportCsvWriter
{
    public async Task<(string fileName, MemoryStream fileContent)> CompileSiteReportCsv(
        IEnumerable<SiteReport> siteReports,
        DateOnly startDate,
        DateOnly endDate)
    {
        var fileName = BuildFileName(startDate, endDate);

        var memoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(memoryStream);
        await CompileCsv(streamWriter, siteReports.ToList());
        return (fileName, memoryStream);
    }

    private string BuildFileName(DateOnly startDate,
        DateOnly endDate) =>
        $"SiteReport_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}_{timeProvider.GetUtcNow():yyyyMMddhhmmss}.csv";

    private static string CsvStringValue(string value) => "\"" + value + "\"";

    private async Task CompileCsv(TextWriter csvWriter, List<SiteReport> siteReports)
    {
        var distinctServices = siteReports.SelectMany(x => x.Bookings.Keys)
            .Union(siteReports.SelectMany(x => x.RemainingCapacity.Keys))
            .Distinct().ToArray();

        await csvWriter.WriteLineAsync(string.Join(',', SiteReportMap.Headers(distinctServices)));

        foreach (var siteReport in siteReports)
        {
            await csvWriter.WriteLineAsync(string.Join(',', CsvStringValue(SiteReportMap.SiteName(siteReport)),
                CsvStringValue(SiteReportMap.Status(siteReport)),
                CsvStringValue(SiteReportMap.SiteType(siteReport)),
                CsvStringValue(SiteReportMap.ICB(siteReport)), CsvStringValue(SiteReportMap.ICBName(siteReport)),
                CsvStringValue(SiteReportMap.Region(siteReport)), CsvStringValue(SiteReportMap.RegionName(siteReport)),
                CsvStringValue(SiteReportMap.OdsCode(siteReport)), SiteReportMap.Longitude(siteReport),
                SiteReportMap.Latitude(siteReport),
                string.Join(',', distinctServices.Select(service => SiteReportMap.BookingsCount(siteReport, service))),
                SiteReportMap.TotalBookings(siteReport).ToString(), SiteReportMap.Cancelled(siteReport).ToString(),
                SiteReportMap.MaximumCapacity(siteReport).ToString(),
                string.Join(',',
                    distinctServices.Select(service => SiteReportMap.CapacityCount(siteReport, service)))));
        }
    }
}
