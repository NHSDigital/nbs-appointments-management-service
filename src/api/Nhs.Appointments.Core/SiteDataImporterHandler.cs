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

        CheckForDuplicatedSites(siteRows, report);
        await CheckIfAnySitesAlreadyExist(siteRows, report);

        var invalidOdsCodes = await GetSitesWithInvalidOdsCodes(siteRows);
        if (invalidOdsCodes.Count > 0)
        {
            report.AddRange(invalidOdsCodes.Select(ods => new ReportItem(-1, ods, false, $"Provided site ODS code: {ods} not found in the well known ODS code list.")));
        }

        if (report.Any(r => !r.Success))
        {
            return report.Where(r => !r.Success);
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

    private static void CheckForDuplicatedSites(List<SiteImportRow> siteRows, List<ReportItem> report)
    {
        var duplicateIds = siteRows.GroupBy(s => s.Id)
            .Where(s => s.Count() > 1)
            .Select(s => s.Key).ToList();

        if (duplicateIds.Count > 0)
        {
            report.AddRange(duplicateIds.Select(dup => new ReportItem(-1, dup, false, $"Duplicate site Id provided: {dup}. SiteIds must be unique.")));
        }

        var duplicateSiteNames = siteRows.GroupBy(s => s.Name)
            .Where(s => s.Count() > 1)
            .Select(s => s.Key).ToList();

        if (duplicateSiteNames.Count > 0)
        {
            report.AddRange(duplicateSiteNames.Select(dup => new ReportItem(-1, dup, false, $"Duplicate site name provided: {dup}. Site names must be unique.")));
        }
    }

    private async Task CheckIfAnySitesAlreadyExist(List<SiteImportRow> siteRows, List<ReportItem> report)
    {
        var existingSiteIds = new List<string>();
        var siteIds = siteRows.Select(s => s.Id).Distinct().ToList();
        foreach (var siteId in siteIds)
        {
            if (await siteService.GetSiteByIdAsync(siteId) is not null)
            {
                existingSiteIds.Add(siteId);
            }
        }

        if (existingSiteIds.Count > 0)
        {
            report.AddRange(existingSiteIds.Select(id => new ReportItem(-1, "Site already exists", false, $"Site with ID: {id} already exists in the system.")));
        }
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
