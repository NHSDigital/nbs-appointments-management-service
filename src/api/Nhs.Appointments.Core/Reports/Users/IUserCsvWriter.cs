using Nhs.Appointments.Core.Users;

namespace Nhs.Appointments.Core.Reports.Users;

public interface IUserCsvWriter
{
    Task<(string fileName, MemoryStream fileContent)> CompileSiteUsersReportCsv(string siteId, IEnumerable<User> users);
}
