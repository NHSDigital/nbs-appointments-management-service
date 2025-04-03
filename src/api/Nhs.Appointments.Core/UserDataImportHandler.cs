using Microsoft.AspNetCore.Http;
using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Core;

public class UserDataImportHandler(
    IUserService userService, 
    ISiteService siteService, 
    IFeatureToggleHelper featureToggleHelper,
    IOktaService oktaService
) : IUserDataImportHandler
{
    public async Task<IEnumerable<ReportItem>> ProcessFile(IFormFile inputFile)
    {
        var oktaEnabled = await featureToggleHelper.IsFeatureEnabled(Flags.OktaEnabled);

        var userImportRows = new List<UserImportRow>();
        var processor = new CsvProcessor<UserImportRow, UserImportRowMap>(
            ui => Task.Run(() => userImportRows.Add(ui)), 
            ui => ui.UserId,
            () => new UserImportRowMap(oktaEnabled) 
        );
        using TextReader fileReader = new StreamReader(inputFile.OpenReadStream());
        var report = (await processor.ProcessFile(fileReader)).ToList();

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
            report.AddRange(incorrectSiteIds.Select(id => new ReportItem(-1, "Incorrect Site ID", false, $"The following site ID doesn't currently exist in the system: {id}.")));
        }

        if (report.Any(r => !r.Success))
        {
            return report.Where(r => !r.Success);
        }

        foreach (var userAssignmentGroup in userImportRows.GroupBy(usr => new { usr.UserId, usr.SiteId }).SelectMany(usr => usr))
        {
            try
            {
                var result = await userService.UpdateUserRoleAssignmentsAsync(userAssignmentGroup.UserId, $"site:{userAssignmentGroup.SiteId}", userAssignmentGroup.RoleAssignments);
                if (!result.Success)
                {
                    report.Add(new ReportItem(-1, userAssignmentGroup.UserId, false, $"Failed to update user roles. The following roles are not valid: {string.Join('|', result.errorRoles)}"));
                }

                if (!userAssignmentGroup.UserId.ToLower().EndsWith("@nhs.net"))
                {
                    var status = await oktaService.CreateIfNotExists(userAssignmentGroup.UserId, userAssignmentGroup.FirstName, userAssignmentGroup.LastName);
                    if (!status.Success)
                    {
                        report.Add(new ReportItem(-1, userAssignmentGroup.UserId, false, $"Failed to create or update OKTA user. Failure reason: {status.FailureReason}"));
                    }
                }
            }
            catch (Exception ex)
            {
                report.Add(new ReportItem(-1, userAssignmentGroup.UserId, false, ex.Message));
            }
        }

        return report;
    }

    public class UserImportRow
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SiteId { get; set; }
        public IEnumerable<RoleAssignment> RoleAssignments { get; set; }
    }
}
