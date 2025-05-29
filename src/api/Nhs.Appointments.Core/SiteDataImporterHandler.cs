using Microsoft.AspNetCore.Http;

namespace Nhs.Appointments.Core;

public class SiteDataImporterHandler(ISiteService siteService, IWellKnowOdsCodesService wellKnowOdsCodesService) : ISiteDataImportHandler
{
    public async Task<IEnumerable<ReportItem>> ProcessFile(IFormFile inputFile)
    {
        var siteRows = new List<SiteImportRow>();
        var processor = new CsvProcessor<SiteImportRow, SiteMap>(
            sr => Task.Run(() => siteRows.Add(sr)), 
            sr => sr.Id,
            () => new SiteMap()
        );
        using TextReader fileReader = new StreamReader(inputFile.OpenReadStream());
        var report = (await processor.ProcessFile(fileReader)).ToList();

        CheckForDuplicatedSites(siteRows, report);
        await CheckIfAnySitesAlreadyExist(siteRows, report);
        await CheckForInvalidSites(siteRows, report);

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
                    site.Accessibilities,
                    site.Type);
            }
            catch (Exception ex)
            {
                report.Add(new ReportItem(-1, site.Id, false, ex.Message));
            }
        }

        return report;
    }

    private async Task CheckForInvalidSites(List<SiteImportRow> siteRows, List<ReportItem> report)
    {
        var wellKnownOdsCodes = await wellKnowOdsCodesService.GetWellKnownOdsCodeEntries();

        var odsCodes = wellKnownOdsCodes.Select(x => x.OdsCode.ToLower());
        var invalidOdsCodes = siteRows
            .Where(s => !odsCodes.Contains(s.OdsCode.ToLower()))
            .Select(s => s.OdsCode)
            .ToList();

        if (invalidOdsCodes.Count > 0)
        {
            report.AddRange(invalidOdsCodes.Select(ods => new ReportItem(-1, "Invalid ODS code", false, $"Provided site ODS code: {ods} not found in the well known ODS code list.")));
        }

        var icbCodes = wellKnownOdsCodes.Where(ods => ods.Type.Equals("icb", StringComparison.CurrentCultureIgnoreCase)).Select(ods => ods.OdsCode.ToLower()).ToList();
        var invalidIcbCodes = siteRows
            .Where(s => !icbCodes.Contains(s.ICB.ToLower()))
            .Select(s => s.ICB)
            .ToList();

        if (invalidIcbCodes.Count > 0)
        {
            report.AddRange(invalidIcbCodes.Select(icb => new ReportItem(-1, "Invalid ICB code", false, $"Provided site ICB code: {icb} not found in the well known ICB code list.")));
        }

        var regions = wellKnownOdsCodes.Where(x => x.Type.Equals("region", StringComparison.CurrentCultureIgnoreCase)).Select(x => x.OdsCode.ToLower()).ToList();
        var invalidRegions = siteRows
            .Where(s => !regions.Contains(s.Region.ToLower()))
            .Select(s => s.Region)
            .ToList();

        if (invalidRegions.Count > 0)
        {
            report.AddRange(invalidRegions.Select(reg => new ReportItem(-1, "Invalid Region", false, $"Provided region: {reg} not found in the well known Region list.")));
        }
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
        public string Type { get; set; }
        public Location Location { get; set; }
        public IEnumerable<Accessibility> Accessibilities { get; set; }
    }
}
