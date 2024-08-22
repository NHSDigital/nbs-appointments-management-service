using Notify.Client;
using Notify.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications
{
    public interface IUserRolesChangedNotifier
    {
        Task Notify(string user, params string[] roles);
    }
    public class UserRolesChangedNotifier : IUserRolesChangedNotifier
    {
        private readonly IAsyncNotificationClient _notificationClient;
        private readonly string _emailTemplateId;

        public UserRolesChangedNotifier(IAsyncNotificationClient notificationClient, string emailTemplateId)
        {
            _notificationClient = notificationClient;
            _emailTemplateId = emailTemplateId;
        }

        public async Task Notify(string user, params string[] roles)
        {
            var templateValues = new Dictionary<string, dynamic>
            {
                {"user", user},
                {"roles", string.Join(", ", roles)}
            };

            await _notificationClient.SendEmailAsync(user, _emailTemplateId, templateValues);
        }
    }
}
