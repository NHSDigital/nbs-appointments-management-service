using Microsoft.Extensions.Options;
using Nhs.Appointments.Core;
using Notify.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications;


public class UserRolesChangedNotifier(IAsyncNotificationClient notificationClient, IRolesStore rolesStore, IOptions<UserRolesChangedNotifier.Options> options) : IUserRolesChangedNotifier
{
    public async Task Notify(string user, string[] rolesAdded, string[] rolesRemoved)
    {
        var roles = await rolesStore.GetRoles();

        var rolesAddedNames = roles.Where(r => rolesAdded.Contains(r.Id)).Select(r => r.Name).ToArray();
        var rolesRemovedNames = roles.Where(r => rolesRemoved.Contains(r.Id)).Select(r => r.Name).ToArray();

        var templateValues = new Dictionary<string, dynamic>
        {
            {"user", user},
            {"rolesAdded", string.Join(", ", rolesAddedNames)},
            {"rolesRemoved", string.Join(", ", rolesRemovedNames)}
        };

        await notificationClient.SendEmailAsync(user, options.Value.EmailTemplateId, templateValues);
    }

    public class Options
    {
        public string EmailTemplateId { get; set; }
    }
}
