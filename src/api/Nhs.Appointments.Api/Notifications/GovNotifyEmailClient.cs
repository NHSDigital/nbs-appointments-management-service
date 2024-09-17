using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications;

public class GovNotifyEmailClient : Notify.Client.NotificationClient, ISendNotifications
{
    public GovNotifyEmailClient(string apiKey) : base(apiKey)
    {
    }

    public async Task SendEmailAsync(string emailAddress, string templateId, Dictionary<string, dynamic> templateValues) => await base.SendEmailAsync(emailAddress, templateId, templateValues);

    public async Task SendSmsAsync(string phoneNumber, string templateId, Dictionary<string, dynamic> templateValues) => await base.SendSmsAsync(phoneNumber, templateId, templateValues);
}
