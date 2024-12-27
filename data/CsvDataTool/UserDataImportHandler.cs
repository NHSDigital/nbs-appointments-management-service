using CsvHelper.Configuration;
using Nhs.Appointments.Persistance.Models;

namespace CsvDataTool;

public class UserDataImportHandler(IFileOperations fileOperations) : IDataImportHandler
{
    public async Task<IEnumerable<ReportItem>> ProcessFile(FileInfo inputFile, DirectoryInfo outputFolder)
    {
        var userImportRows = new List<UserImportRow>();
        var processor = new CsvProcessor<UserImportRow, UserImportRowMap>(ui => Task.Run(() => userImportRows.Add(ui)), ui => ui.UserId);
        using var fileReader = fileOperations.OpenText(inputFile);
        var report = (await processor.ProcessFile(fileReader)).ToList();

        string[] rolesToAssign = ["canned:site-details-manager", "canned:user-manager"];
        
        foreach (var userAssignmentGroup in userImportRows.GroupBy(usr => usr.UserId))
        {
            var userDocument = new UserDocument
            {
                Id = userAssignmentGroup.Key,
                DocumentType = "user",
                RoleAssignments = userAssignmentGroup
                    .SelectMany(ua => rolesToAssign
                    .Select(r => new RoleAssignment { Role = r, Scope = $"site:{ua.SiteId}" }))
                    .ToArray()
            };

            try
            {
                await WriteUserDocument(userDocument, outputFolder);
            }
            catch (Exception ex)
            {
                report.Add(new ReportItem(-1, userDocument.Id, false, ex.Message));
            }
        }        

        return report;
    }

    private Task WriteUserDocument(UserDocument userDocument, DirectoryInfo outputDirectory)
    {
        fileOperations.CreateFolder(outputDirectory);
        var filePath = Path.Combine(outputDirectory.FullName, $"user_{userDocument.Id.Replace('@', '_').Replace('.', '_')}.json");
        return fileOperations.WriteDocument(userDocument, filePath);
    }

    private class UserImportRow
    {
        public string UserId { get; set; }
        public string SiteId { get; set; }
    }

    private class UserImportRowMap : ClassMap<UserImportRow>
    {
        public UserImportRowMap()
        {
            Map(m => m.UserId).Name("User");
            Map(m => m.SiteId).Name("Site");
        }
    }
}
