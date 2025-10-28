using Microsoft.AspNetCore.Http;

namespace Nhs.Appointments.Core.BulkImport;
public class SiteStatusDataImportHandler(ISiteService siteService) : ISiteStatusDataImportHandler
{
    public async Task<IEnumerable<ReportItem>> ProcessFile(IFormFile inputFile)
    {
        var siteRows = new List<SiteStatusImportRow>();
        var processor = new CsvProcessor<SiteStatusImportRow, SiteStatusImportRowMap>(
            sr => Task.Run(() => siteRows.Add(sr)),
            sr => sr.Id,
            () => new SiteStatusImportRowMap()
        );
        using TextReader fileReader = new StreamReader(inputFile.OpenReadStream());
        var report = (await processor.ProcessFile(fileReader)).ToList();

        // Get a list of all sites
        // For each site in the report
            // Check it exists
            // Check the name matches

        return report;
    }

    public class SiteStatusImportRow
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
