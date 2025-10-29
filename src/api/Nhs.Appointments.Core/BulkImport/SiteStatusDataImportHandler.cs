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

        var sites = await siteService.GetAllSites(includeDeleted: true);

        foreach (var row in siteRows)
        {
            try
            {
                var matchingSite = sites.FirstOrDefault(s => s.Id == row.Id && s.Name.Equals(row.Name, StringComparison.CurrentCultureIgnoreCase));
                if (matchingSite is null)
                {
                    report.Add(new ReportItem(-1, row.Name, false, $"Could not find existing site with name: {row.Name} and ID: {row.Id}."));
                    continue;
                }

                var result = await siteService.ToggleSiteSoftDeletionAsync(row.Id);
                if (!result.Success)
                {
                    report.Add(new ReportItem(-1, row.Name, false, $"Failed to update the soft deletion status of site with name: {row.Name} and ID: {row.Id}"));
                    continue;
                }
            }
            catch (Exception ex)
            {
                report.Add(new ReportItem(-1, row.Name, false, ex.Message));
            }
        }

        return report;
    }

    public class SiteStatusImportRow
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
