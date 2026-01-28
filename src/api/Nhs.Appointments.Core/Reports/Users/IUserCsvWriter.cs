using Nhs.Appointments.Core.Users;

namespace Nhs.Appointments.Core.Reports.Users;

public interface IUserCsvWriter
{
    Task<(string FileName, MemoryStream FileContent)> CompileSiteUsersReportCsv(string siteId, IEnumerable<User> users);
}
