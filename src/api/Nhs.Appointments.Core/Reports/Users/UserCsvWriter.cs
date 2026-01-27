using Nhs.Appointments.Core.Users;

namespace Nhs.Appointments.Core.Reports.Users;

public class UserCsvWriter(TimeProvider timeProvider) : IUserCsvWriter
{
    private readonly string[] Headers = ["User"];

    public async Task<(string fileName, MemoryStream fileContent)> CompileSiteUsersReportCsv(string siteId, IEnumerable<User> users)
    {
        var fileName = $"UserReport_Site_{siteId}_{timeProvider.GetUtcNow():yyyyMMddhhmmss}.csv";

        var memoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(memoryStream);
        await CompileUsersCsv(streamWriter, users);
        return (fileName, memoryStream);
    }

    private async Task CompileUsersCsv(StreamWriter csvWriter, IEnumerable<User> users)
    {
        await csvWriter.WriteLineAsync(string.Join(',', Headers));

        foreach (var user in users)
        {
            await csvWriter.WriteLineAsync(user.Id);
        }
    }
}
