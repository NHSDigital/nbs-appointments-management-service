using Nhs.Appointments.Core.Sites;

namespace Nhs.Appointments.Core.Reports.MasterSiteList;

public class MasterSiteListReportCsvWriter(TimeProvider timeProvider) : IMasterSiteListReportCsvWriter
{
    public async Task<(string fileName, MemoryStream fileContent)> CompileMasterSiteListReportCsv(IEnumerable<Site> sites)
    {
        var fileName = BuildFileName();

        var memoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(memoryStream);
        await CompileCsv(streamWriter, sites);
        return (fileName, memoryStream);
    }

    private string BuildFileName() =>
    $"MasterSiteListReport_{timeProvider.GetUtcNow():yyyyMMddhhmmss}.csv";

    private static string CsvStringValue(string value) => "\"" + value + "\"";

    private async Task CompileCsv(TextWriter csvWriter, IEnumerable<Site> sites)
    {
        await csvWriter.WriteLineAsync(string.Join(',', MasterSiteListReportMap.Headers()));

        foreach (var site in sites)
        {
            try
            {
                await csvWriter.WriteLineAsync(string.Join(',', 
                    CsvStringValue(MasterSiteListReportMap.SiteName(site)),
                    CsvStringValue(MasterSiteListReportMap.OdsCode(site)), 
                    CsvStringValue(MasterSiteListReportMap.SiteType(site)),
                    CsvStringValue(MasterSiteListReportMap.Region(site)),
                    CsvStringValue(MasterSiteListReportMap.ICB(site)),
                    CsvStringValue(MasterSiteListReportMap.Guid(site)),
                    MasterSiteListReportMap.IsDeleted(site),
                    CsvStringValue(MasterSiteListReportMap.Status(site)),
                    MasterSiteListReportMap.Longitude(site),
                    MasterSiteListReportMap.Latitude(site),
                    CsvStringValue(MasterSiteListReportMap.Address(site))
                   ));

            }
            catch(Exception ex)
            {

            }
        }
    }
}
