using MassTransit.Configuration;
using Microsoft.Extensions.Options;
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

        public UserRolesChangedNotifier(IAsyncNotificationClient notificationClient, IOptions<UserRolesChangedNotifier.Options> options)
        {
            _notificationClient = notificationClient;
            _emailTemplateId = options.Value.EmailTemplateId;
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

        public class Options
        {
            public string EmailTemplateId { get; set; }
        }
    }
}
