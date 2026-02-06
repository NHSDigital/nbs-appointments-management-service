using Nhs.Appointments.Core.Sites;

namespace Nhs.Appointments.Core.Reports.MasterSiteList;

public interface IMasterSiteListReportCsvWriter
{
    Task<(string fileName, MemoryStream fileContent)> CompileMasterSiteListReportCsv(IEnumerable<Site> sites);
}
