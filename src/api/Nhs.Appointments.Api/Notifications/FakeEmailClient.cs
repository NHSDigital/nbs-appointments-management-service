using DnsClient.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications;

public class FakeEmailClient(ILogger logger) : ISendEmails
{
    public Task SendEmailAsync(string emailAddress, string templateId, Dictionary<string, dynamic> templateValues)
    {
        var vals = string.Join(", ", templateValues.Select(x => $"{x.Key}:{x.Value}"));
        logger.LogInformation($"Sending notification to {emailAddress} using template id {templateId}. Template values: {vals}");
        return Task.CompletedTask;
    }
}
