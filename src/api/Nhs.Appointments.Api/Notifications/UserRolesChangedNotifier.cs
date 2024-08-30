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
        var roles = await rolesStore.GetRoles();
        var site = await siteService.GetSiteByIdAsync(siteId);

        var rolesAddedNames = roles.Where(r => rolesAdded.Contains(r.Id)).Select(r => r.Name).ToArray();
        var rolesRemovedNames = roles.Where(r => rolesRemoved.Contains(r.Id)).Select(r => r.Name).ToArray();

        var templateValues = new Dictionary<string, dynamic>
        {
            {"user", user},
            {"rolesAdded", string.Join(", ", rolesAddedNames)},
            {"rolesRemoved", string.Join(", ", rolesRemovedNames)},
            {"site", site?.Name }
        };

        await notificationClient.SendEmailAsync(user, options.Value.EmailTemplateId, templateValues);
    }

    public class Options
    {
        public string EmailTemplateId { get; set; }
    }
}
