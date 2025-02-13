using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;

namespace Nhs.Appointments.Core;

public interface IDataImportHandler
{
    Task<IEnumerable<ReportItem>> ProcessFile(IFormFile inputFile);
}

public interface IUserDataImportHandler : IDataImportHandler { }

public class UserDataImportHandler(IUserService userService, ISiteService siteService) : IUserDataImportHandler
{
    public async Task<IEnumerable<ReportItem>> ProcessFile(IFormFile inputFile)
    {
        var userImportRows = new List<UserImportRow>();
        var processor = new CsvProcessor<UserImportRow, UserImportRowMap>(ui => Task.Run(() => userImportRows.Add(ui)), ui => ui.UserId);
        using TextReader fileReader = new StreamReader(inputFile.OpenReadStream());
        var report = (await processor.ProcessFile(fileReader)).ToList();

        string[] rolesToAssign = ["canned:site-details-manager", "canned:user-manager", "canned:availability-manager", "canned:appointment-manager"];

        var incorrectSiteIds = new List<string>();
        var sites = userImportRows.Select(usr => usr.SiteId).Distinct().ToList();
        foreach (var site in sites)
        {
            if (await siteService.GetSiteByIdAsync(site) is null)
            {
                incorrectSiteIds.Add(site);
            }
        }

        if (incorrectSiteIds.Count > 0)
        {
            report.Add(new ReportItem(-1, "Incorrect Site IDs", false, $"The sites with these IDs don't currently exist in the system. {String.Join(',', incorrectSiteIds)}"));
            return report;
        }

        foreach (var userAssignmentGroup in userImportRows.GroupBy(usr => usr.UserId))
        {
            try
            {
                var roleAssignments = userAssignmentGroup
                    .SelectMany(ua => rolesToAssign
                    .Select(r => new RoleAssignment { Role = r, Scope = $"site:{ua.SiteId}" }));
                // TODO: We can use the UpdateUserRoleAssignmentsAsync method, but this sends out notifications - do we want this initially?
                await userService.SaveUserAsync(userAssignmentGroup.Key, "site", roleAssignments);
            }
            catch (Exception ex)
            {
                report.Add(new ReportItem(-1, userAssignmentGroup.Key, false, ex.Message));
            }
        }

        return report;
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
            Map(m => m.UserId)
                .Name("User")
                .Validate(f => !string.IsNullOrWhiteSpace(f.Field));
            Map(m => m.SiteId)
                .Name("Site")
                .Validate(f => !string.IsNullOrWhiteSpace(f.Field));
        }
    }
}
