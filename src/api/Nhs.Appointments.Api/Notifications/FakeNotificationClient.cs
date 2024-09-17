using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications;

public class FakeNotificationClient(ILogger<FakeNotificationClient> logger) : ISendNotifications
{
    public Task SendEmailAsync(string emailAddress, string templateId, Dictionary<string, dynamic> templateValues)
    {
        var vals = string.Join(", ", templateValues.Select(x => $"{x.Key}:{x.Value}"));
        logger.LogInformation($"Sending email notification to {emailAddress} using template id {templateId}. Template values: {vals}");
        return Task.CompletedTask;
    }

    public Task SendSmsAsync(string phoneNumber, string templateId, Dictionary<string, dynamic> templateValues)
    {
        var vals = string.Join(", ", templateValues.Select(x => $"{x.Key}:{x.Value}"));
        logger.LogInformation($"Sending SMS notification to {phoneNumber} using template id {templateId}. Template values: {vals}");
        return Task.CompletedTask;
    }
}
