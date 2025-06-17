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

        var regionalUsers = userImportRows.Where(usr => !string.IsNullOrEmpty(usr.Region)).ToList();
        if (regionalUsers.Count > 0)
        {
            await ValidateRegionCodes(regionalUsers, report);
            CheckForDuplicateRegionPermissions(regionalUsers, report);
        }

        if (report.Any(r => !r.Success))
        {
            return report.Where(r => !r.Success);
        }

        foreach (var userRow in userImportRows)
        {
            try
            {
                if (!userRow.UserId.EndsWith("@nhs.net", StringComparison.OrdinalIgnoreCase))
                {
                    var status = await oktaService.CreateIfNotExists(userRow.UserId, userRow.FirstName, userRow.LastName);
                    if (!status.Success)
                    {
                        report.Add(new ReportItem(-1, userRow.UserId, false, $"Failed to create or update OKTA user. Failure reason: {status.FailureReason}"));
                        continue;
                    }
                }

                var isRegionPermission = !string.IsNullOrEmpty(userRow.Region);
                if (isRegionPermission)
                {
                    await userService.UpdateRegionalUserRoleAssignmentsAsync(userRow.UserId, $"region:{userRow.Region}", userRow.RoleAssignments);
                }
                else
                {
                    var result = await userService.UpdateUserRoleAssignmentsAsync(userRow.UserId, $"site:{userRow.SiteId}", userRow.RoleAssignments, false);
                    if (!result.Success)
                    {
                        report.Add(new ReportItem(-1, userRow.UserId, false, $"Failed to update user roles. The following roles are not valid: {string.Join('|', result.errorRoles)}"));
                    }
                }
            }
            catch (Exception ex)
            {
                report.Add(new ReportItem(-1, userRow.UserId, false, ex.Message));
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

    private static void CheckForDuplicateRegionPermissions(List<UserImportRow> userImportRows, List<ReportItem> report)
    {
        var duplicateUsers = userImportRows
            .GroupBy(usr => usr.UserId)
            .Where(usr => usr.Count() > 1)
            .ToList();

        if (duplicateUsers.Count > 0)
        {
            report.AddRange(duplicateUsers.Select(usr => new ReportItem(
                -1,
                "User added to multiple regions",
                false,
                $"Users can only be added to one region per upload. User: {usr.Key} has been added multiple times for region scoped permissions.")));
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
