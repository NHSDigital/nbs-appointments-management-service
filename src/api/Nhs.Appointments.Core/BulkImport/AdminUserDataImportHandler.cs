using Microsoft.AspNetCore.Http;

namespace Nhs.Appointments.Core.BulkImport;
public class AdminUserDataImportHandler(IUserService userService) : IAdminUserDataImportHandler
{
    public async Task<IEnumerable<ReportItem>> ProcessFile(IFormFile inputFile)
    {
        var adminUserImportRows = new List<AdminUserImportRow>();
        var processor = new CsvProcessor<AdminUserImportRow, AdminUserImportRowMap>(
            aui => Task.Run(() => adminUserImportRows.Add(aui)),
            aui => aui.Email,
            () => new AdminUserImportRowMap());

        using TextReader fileReader = new StreamReader(inputFile.OpenReadStream());
        var report = (await processor.ProcessFile(fileReader)).ToList();

        if (report.Any(r => !r.Success))
        {
            return report.Where(r => !r.Success);
        }

        foreach (var row in adminUserImportRows)
        {
            try
            {
                var lowerUserId = row.Email.ToLower();

                var user = await userService.GetUserAsync(lowerUserId);

                if (user is null)
                {
                    await userService.SaveAdminUserAsync(lowerUserId);
                }
                else
                {
                    await userService.RemoveAdminUserAsync(lowerUserId);
                }
            }
            catch (Exception ex)
            {
                report.Add(new ReportItem(-1, row.Email, false, ex.Message));
            }
        }

        return report;
    }

    public class AdminUserImportRow
    {
        public string Email { get; set; }
    }
}
