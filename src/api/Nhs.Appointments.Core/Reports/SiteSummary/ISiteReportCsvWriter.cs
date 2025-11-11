using Nhs.Appointments.Core.Sites;

namespace Nhs.Appointments.Core.Reports.SiteSummary;

public interface ISiteReportCsvWriter
{
    Task<(string fileName, MemoryStream fileContent)> CompileSiteReportCsv(
        IEnumerable<SiteReport> siteReports,
        DateOnly startDate,
        DateOnly endDate);
}
