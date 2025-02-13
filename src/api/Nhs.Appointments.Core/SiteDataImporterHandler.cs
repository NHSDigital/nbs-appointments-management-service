using Microsoft.AspNetCore.Http;

namespace Nhs.Appointments.Core;

public interface ISiteDataImportHandler : IDataImportHandler { }

public class SiteDataImporterHandler(ISiteService siteService, IWellKnowOdsCodesService wellKnowOdsCodesService) : ISiteDataImportHandler
{
    public async Task<IEnumerable<ReportItem>> ProcessFile(IFormFile inputFile)
    {
        var siteRows = new List<SiteImportRow>();
        var processor = new CsvProcessor<SiteImportRow, SiteMap>(sr => Task.Run(() => siteRows.Add(sr)), sr => sr.Id);
        using TextReader fileReader = new StreamReader(inputFile.OpenReadStream());
        var report = (await processor.ProcessFile(fileReader)).ToList();

        var distinctIds = siteRows.GroupBy(s => s.Id).Count();
        if (siteRows.Count != distinctIds)
        {
            report.Add(new ReportItem(-1, "Duplicate site IDs", false, "Document contains duplicated siteIds. These IDs must be unique."));
            return report;
        }

        var wellKnownOdsCodes = (await wellKnowOdsCodesService.GetWellKnownOdsCodeEntries())
            .Select(ods => ods.OdsCode)
            .ToList();

        foreach (var site in siteRows)
        {
            try
            {
                // TODO: Should be break in this case? Or skip over this site and carry on with the rest?
                if (!wellKnownOdsCodes.Contains(site.OdsCode))
                {
                    report.Add(new ReportItem(-1, site.OdsCode, false, "Provided site ODS not found in the well known ODS code list."));
                    break;
                }

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
