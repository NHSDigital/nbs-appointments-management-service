using Microsoft.AspNetCore.Http;
using Nhs.Appointments.Core.Sites;

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

        var sites = await siteService.GetAllSites(includeDeleted: true, ignoreCache: true);

        var missingSites = FindMissingSites(siteRows, sites);
        if (missingSites.Count > 0)
        {
            report.AddRange(missingSites.Select(ms => new ReportItem(-1, ms.Name, false, $"Could not find existing site with name {ms.Name} and ID: {ms.Id}")));
        }

        if (report.Any(r => !r.Success))
        {
            return report.Where(r => !r.Success);
        }

        foreach (var row in siteRows)
        {
            try
            {
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

    public static List<SiteStatusImportRow> FindMissingSites(List<SiteStatusImportRow> siteRows, IEnumerable<Site> sites)
    {
        return [.. siteRows.Where(sr => !sites.Any(s => s.Id == sr.Id && s.Name.Equals(sr.Name, StringComparison.CurrentCultureIgnoreCase)))];
    }

    public class SiteStatusImportRow
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
