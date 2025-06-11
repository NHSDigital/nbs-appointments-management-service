using Microsoft.AspNetCore.Http;
using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Core;

public class UserDataImportHandler(
    IUserService userService, 
    ISiteService siteService, 
    IFeatureToggleHelper featureToggleHelper,
    IOktaService oktaService,
    IEmailWhitelistStore emailWhitelistStore,
    IWellKnowOdsCodesService wellKnowOdsCodesService
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
        var sites = userImportRows
            .Where(usr => !string.IsNullOrEmpty(usr.SiteId))
            .Select(usr => usr.SiteId)
            .Distinct()
            .ToList();
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

        await CheckForNonWhitelistedEmailDomains(userImportRows, report);
        await ValidateRegionCodes([.. userImportRows.Where(usr => !string.IsNullOrEmpty(usr.Region))], report);

        if (report.Any(r => !r.Success))
        {
            return report.Where(r => !r.Success);
        }

        foreach (var userAssignmentGroup in userImportRows.GroupBy(usr => usr.UserId).SelectMany(usr => usr))
        {
            try
            {
                if (!userAssignmentGroup.UserId.ToLower().EndsWith("@nhs.net"))
                {
                    var status = await oktaService.CreateIfNotExists(userAssignmentGroup.UserId, userAssignmentGroup.FirstName, userAssignmentGroup.LastName);
                    if (!status.Success)
                    {
                        report.Add(new ReportItem(-1, userAssignmentGroup.UserId, false, $"Failed to create or update OKTA user. Failure reason: {status.FailureReason}"));
                        continue;
                    }
                }

                var scope = string.IsNullOrEmpty(userAssignmentGroup.SiteId)
                    ? $"region:{userAssignmentGroup.Region}"
                    : $"site:{userAssignmentGroup.SiteId}";

                var result = await userService.UpdateUserRoleAssignmentsAsync(userAssignmentGroup.UserId, scope, userAssignmentGroup.RoleAssignments);
                if (!result.Success)
                {
                    report.Add(new ReportItem(-1, userAssignmentGroup.UserId, false, $"Failed to update user roles. The following roles are not valid: {string.Join('|', result.errorRoles)}"));
                }
            }
            catch (Exception ex)
            {
                report.Add(new ReportItem(-1, userAssignmentGroup.UserId, false, ex.Message));
            }
        }

        return report;
    }

    private async Task CheckForNonWhitelistedEmailDomains(List<UserImportRow> userImportRows, List<ReportItem> report)
    {
        var whitelistedEmails = await emailWhitelistStore.GetWhitelistedEmails();
        var invalidEmails = userImportRows
            .Where(u => !whitelistedEmails.Any(email => u.UserId.EndsWith(email.Trim(), StringComparison.InvariantCultureIgnoreCase)))
            .ToList();

        if (invalidEmails.Count > 0)
        {
            report.AddRange(invalidEmails.Select(usr => new ReportItem(-1, "Invalid email domain", false, $"The following email domain: {usr.UserId} is not included in the email domain whitelist.")));
        }
    }

    private async Task ValidateRegionCodes(List<UserImportRow> userImportRows, List<ReportItem> report)
    {
        if (userImportRows.Count == 0)
            return;

        var wellKnownOdsCodes = await wellKnowOdsCodesService.GetWellKnownOdsCodeEntries();
        var regionCodes = wellKnownOdsCodes.Where(ods => ods.Type.Equals("region", StringComparison.CurrentCultureIgnoreCase)).Select(x => x.OdsCode.ToLower()).ToList();

        var invalidRegions = userImportRows
            .Where(usr => !regionCodes.Contains(usr.Region.ToLower()))
            .Select(usr => usr.Region)
            .ToList();

        if (invalidRegions.Count > 0)
        {
            report.AddRange(invalidRegions.Select(reg => new ReportItem(-1, "Invalid Region", false, $"Provided region: {reg} not found in the well known Region list.")));
        }
    }

    public class UserImportRow
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SiteId { get; set; }
        public IEnumerable<RoleAssignment> RoleAssignments { get; set; }
        public string Region { get; set; }
    }
}
