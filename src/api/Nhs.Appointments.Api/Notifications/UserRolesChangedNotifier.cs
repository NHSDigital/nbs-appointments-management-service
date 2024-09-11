using Microsoft.Extensions.Options;
using Nhs.Appointments.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications;

// Depends on an email template hosted in Gov Notify. See template.txt for details 
public class UserRolesChangedNotifier(ISendEmails notificationClient, IRolesStore rolesStore, ISiteSearchService siteService, IOptions<UserRolesChangedNotifier.Options> options) : IUserRolesChangedNotifier
{
    public async Task Notify(string user, string siteId, string[] rolesAdded, string[] rolesRemoved)
    {
        var hasRoleChanges = rolesAdded.Any() || rolesRemoved.Any();

        if (hasRoleChanges)
        {
            var roles = await rolesStore.GetRoles();
            var rolesAddedNames = GetRoleNames(roles, rolesAdded);
            var rolesRemovedNames = GetRoleNames(roles, rolesRemoved);

            var site = await siteService.GetSiteByIdAsync(siteId);
            var siteName = site == null ? $"Unknown site ({siteId})" : site.Name;

            var templateValues = new Dictionary<string, dynamic>
        {
            {"user", user},
            {"rolesAdded", GetRolesAddedText(rolesAddedNames)},
            {"rolesRemoved", GetRolesRemovedText(rolesRemovedNames)},
            {"site", siteName }
        };

            await notificationClient.SendEmailAsync(user, options.Value.EmailTemplateId, templateValues);
        }
    }

    private string[] GetRoleNames(IEnumerable<Role> roles, IEnumerable<string> roleIds)
    {
        return roles.Where(role => 
            roleIds.Contains(role.Id) ||
            roleIds.Select(GetRoleIdPortion).Contains(role.Id)

        ).Select(role => role.Name).ToArray();
    }

    // we assemble some text here because the Gov Notify templating engine doesn't quite do what we need:
    private static string GetRolesAddedText(string[] rolesAdded)
    {
        if (rolesAdded.Length == 0) return "";

        return $"You have been added to: {string.Join(", ", rolesAdded)}.";
    }

    private static string GetRolesRemovedText(string[] rolesRemoved)
    {
        if (rolesRemoved.Length == 0) return "";

        return $"You have been removed from: {string.Join(", ", rolesRemoved)}.";
    }

    private static string GetRoleIdPortion(string roleName)
    {
        if(roleName.Contains(':')) return roleName.Split(':')[1];

        return roleName;
    }

    public class Options
    {
        public string EmailTemplateId { get; set; }
    }
}
