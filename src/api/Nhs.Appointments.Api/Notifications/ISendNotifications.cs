using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications;

public interface ISendNotifications
{
    Task SendEmailAsync(string emailAddress, string templateId, Dictionary<string, dynamic> templateValues);
    Task SendSmsAsync(string phoneNumber, string templateId, Dictionary<string, dynamic> templateValues);
}
