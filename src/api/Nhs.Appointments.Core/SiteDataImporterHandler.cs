using Microsoft.AspNetCore.Http;

namespace Nhs.Appointments.Core;

public class SiteDataImporterHandler(ISiteService siteService, IWellKnowOdsCodesService wellKnowOdsCodesService) : ISiteDataImportHandler
{
    public async Task<IEnumerable<ReportItem>> ProcessFile(IFormFile inputFile)
    {
        var siteRows = new List<SiteImportRow>();
        var processor = new CsvProcessor<SiteImportRow, SiteMap>(sr => Task.Run(() => siteRows.Add(sr)), sr => sr.Id);
        using TextReader fileReader = new StreamReader(inputFile.OpenReadStream());
        var report = (await processor.ProcessFile(fileReader)).ToList();

        var duplicateIds = siteRows.GroupBy(s => s.Id)
            .Where(s => s.Count() > 1)
            .Select(s => s.Key).ToList();

        if (duplicateIds.Count > 0)
        {
            report.Clear();
            report.AddRange(duplicateIds.Select(dup => new ReportItem(-1, dup, false, $"Duplicate site Id provided: {dup}. SiteIds must be unique.")));
            return report;
        }

        var invalidOdsCodes = await GetSitesWithInvalidOdsCodes(siteRows);
        if (invalidOdsCodes.Count > 0)
        {
            report.Clear();
            report.AddRange(invalidOdsCodes.Select(ods => new ReportItem(-1, ods, false, $"Provided site ODS code: {ods} not found in the well known ODS code list.")));
            return report;
        }

        foreach (var site in siteRows)
        {
            try
            {
                await siteService.SaveSiteAsync(
                    site.Id,
                    site.OdsCode,
                    site.Name,
                    site.Address,
                    site.PhoneNumber,
                    site.ICB,
                    site.Region,
                    site.Location,
                    site.Accessibilities);
            }
            catch (Exception ex)
            {
                report.Add(new ReportItem(-1, site.Id, false, ex.Message));
            }
        }

        return report;
    }

    private async Task<List<string>> GetSitesWithInvalidOdsCodes(List<SiteImportRow> siteRows)
    {
        var wellKnownOdsCodes = (await wellKnowOdsCodesService.GetWellKnownOdsCodeEntries())
            .Select(ods => ods.OdsCode.ToLower())
            .ToList();

        return siteRows.Where(s => !wellKnownOdsCodes.Contains(s.OdsCode.ToLower()))
            .Select(s => s.OdsCode)
            .ToList();
    }

    public class SiteImportRow
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string OdsCode { get; set; }
        public string Region { get; set; }
        public string ICB { get; set; }
        public Location Location { get; set; }
        public IEnumerable<Accessibility> Accessibilities { get; set; }
    }
}
