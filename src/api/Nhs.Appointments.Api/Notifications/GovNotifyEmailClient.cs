using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications;

public class GovNotifyEmailClient : Notify.Client.NotificationClient, ISendEmails
{
    public GovNotifyEmailClient(string apiKey) : base(apiKey)
    {
    }

    public async Task SendEmailAsync(string emailAddress, string templateId, Dictionary<string, dynamic> templateValues) => await base.SendEmailAsync(emailAddress, templateId, templateValues);
}
