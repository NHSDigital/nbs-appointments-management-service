using CsvHelper.Configuration;
using Nhs.Appointments.Persistance.Models;

namespace CsvDataTool;

public class ApiUserDataImportHandler(IFileOperations fileOperations) : IDataImportHandler
{
    public Task<IEnumerable<ReportItem>> ProcessFile(FileInfo inputFile, DirectoryInfo outputFolder)
    {
        var processor = new CsvProcessor<ApiUserImportRow, ApiUserImportRowMap>(ai => WriteApiUserDocument(ai, outputFolder), ui => ui.ClientId);
        using var fileReader = fileOperations.OpenText(inputFile);
        return processor.ProcessFile(fileReader);
    }

    private Task WriteApiUserDocument(ApiUserImportRow apiUserRow, DirectoryInfo outputDirectory)
    {
        var userDocument = new UserDocument
        {
            Id = $"api@{apiUserRow.ClientId}",
            DocumentType = "user",
            ApiSigningKey = apiUserRow.ApiSigningKey,
            RoleAssignments =                 [
                new RoleAssignment()
                    { Role = "system:api-user", Scope = "global" }
            ],

        };
        fileOperations.CreateFolder(outputDirectory);
        var filePath = Path.Combine(outputDirectory.FullName, $"user_{userDocument.Id.Replace('@', '_').Replace('.', '_')}.json");
        return fileOperations.WriteDocument(userDocument, filePath);
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
            Map(m => m.ClientId).Name("ClientId");
            Map(m => m.ApiSigningKey).Name("ApiSigningKey");
        }
    }
}
