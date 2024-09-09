using Microsoft.Extensions.Options;
using Nhs.Appointments.Core;
using Notify.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications;

// Depends on an email template hosted in Gov Notify. See template.txt for details 
public class UserRolesChangedNotifier(IAsyncNotificationClient notificationClient, IRolesStore rolesStore, ISiteSearchService siteService, IOptions<UserRolesChangedNotifier.Options> options) : IUserRolesChangedNotifier
{
    public async Task Notify(string user, string siteId, string[] rolesAdded, string[] rolesRemoved)
    {
        if(rolesAdded.Length == 0 && rolesRemoved.Length == 0)
        {
            // avoid sending pointless notifications when nothing has actually changed:
            return;
        }

        var roles = await rolesStore.GetRoles();
        var site = await siteService.GetSiteByIdAsync(siteId);
        var siteName = site == null ? $"Unknown site ({siteId})" : site.Name;

        var templateValues = new Dictionary<string, dynamic>
        {
            {"user", user},
            {"rolesAdded", GetRolesAddedText(rolesAdded)},
            {"rolesRemoved", GetRolesRemovedText(rolesRemoved)},
            {"site", siteName }
        };

        await notificationClient.SendEmailAsync(user, options.Value.EmailTemplateId, templateValues);
    }

    // we assemble some text here because the Gov Notify templating engine doesn't quite do what we need:
    private static string GetRolesAddedText(string[] rolesAdded)
    {
        if (rolesAdded.Length == 0) return "";

        return $"You have been added to: {string.Join(", ", rolesAdded.Select(GetFriendlyRoleName))}.";
    }

    private static string GetRolesRemovedText(string[] rolesRemoved)
    {
        if (rolesRemoved.Length == 0) return "";

        return $"You have been removed from: {string.Join(", ", rolesRemoved.Select(GetFriendlyRoleName))}.";
    }

    private static string GetFriendlyRoleName(string roleName)
    {
        if(roleName.Contains(':')) return roleName.Split(':')[1];

        return roleName;
    }

    public class Options
    {
        public string EmailTemplateId { get; set; }
    }
}
