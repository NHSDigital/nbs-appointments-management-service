using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Nhs.Appointments.Core.BulkImport;

namespace Nhs.Appointments.Core;

public class ApiUserDataImportHandler(IUserService userService) : IApiUserDataImportHandler
{
    public async Task<IEnumerable<ReportItem>> ProcessFile(IFormFile inputFile)
    {
        var apiUserImportRows = new List<ApiUserImportRow>();
        var processor = new CsvProcessor<ApiUserImportRow, ApiUserImportRowMap>(
            aui => Task.Run(() => apiUserImportRows.Add(aui)), 
            aui => aui.ClientId,
            () => new ApiUserImportRowMap()
        );
        using TextReader fileReader = new StreamReader(inputFile.OpenReadStream());
        var report = (await processor.ProcessFile(fileReader)).ToList();

        foreach (var apiUser in apiUserImportRows)
        {
            try
            {
                var userId = $"api@{apiUser.ClientId}";
                var roleAssignments = new List<RoleAssignment>
                {
                    new() { Role = "system:api-user", Scope = "global" }
                };
                await userService.SaveUserAsync(userId, "global", roleAssignments);
            }
            catch (Exception ex)
            {
                report.Add(new ReportItem(-1, apiUser.ClientId, false, ex.Message));
            }
        }

        return report;
    }

    private class ApiUserImportRow
    {
        public string ClientId { get; set; }
        public string ApiSigningKey { get; set; }
    }

    private sealed class ApiUserImportRowMap : ClassMap<ApiUserImportRow>
    {
        public ApiUserImportRowMap()
        {
            Map(m => m.ClientId)
                .Name("ClientId")
                .Validate(f => !string.IsNullOrWhiteSpace(f.Field));
            Map(m => m.ApiSigningKey)
                .Name("ApiSigningKey")
                .Validate(f => !string.IsNullOrWhiteSpace(f.Field));
        }
    }
}
