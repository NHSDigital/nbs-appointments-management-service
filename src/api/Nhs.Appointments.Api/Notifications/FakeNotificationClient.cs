using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nhs.Appointments.Api.Notifications;

public class FakeNotificationClient(ILogger<FakeNotificationClient> logger) : ISendNotifications
{
    public Task<bool> SendEmailAsync(string emailAddress, string templateId, Dictionary<string, dynamic> templateValues)
    {
        if (string.IsNullOrEmpty(emailAddress)) throw new ArgumentException("Email address cannot be empty", nameof(emailAddress));
        if (string.IsNullOrEmpty(templateId)) throw new ArgumentException("Template id cannot be empty", nameof(templateId));

        var vals = string.Join(", ", templateValues.Select(x => $"{x.Key}:{x.Value}"));
        logger.LogInformation($"Sending email notification to {emailAddress} using template id {templateId}. Template values: {vals}");
        return Task.FromResult(true);
    }

    public Task<bool> SendSmsAsync(string phoneNumber, string templateId, Dictionary<string, dynamic> templateValues)
    {
        if (string.IsNullOrEmpty(phoneNumber)) throw new ArgumentException("Phone number cannot be empty", nameof(phoneNumber));
        if (string.IsNullOrEmpty(templateId)) throw new ArgumentException("Template id cannot be empty", nameof(templateId));

        var vals = string.Join(", ", templateValues.Select(x => $"{x.Key}:{x.Value}"));
        logger.LogInformation($"Sending SMS notification to {phoneNumber} using template id {templateId}. Template values: {vals}");
        return Task.FromResult(true);
    }
}

public class CosmosNotificationClient(CosmosClient cosmosClient, IOptions<CosmosDataStoreOptions> options) : ISendNotifications
{
    public async Task<bool> SendEmailAsync(string emailAddress, string templateId, Dictionary<string, dynamic> templateValues) => await WriteNotification(emailAddress, templateId, templateValues);
    
    public async Task<bool> SendSmsAsync(string phoneNumber, string templateId, Dictionary<string, dynamic> templateValues) => await WriteNotification(phoneNumber, templateId, templateValues);

    private async Task<bool> WriteNotification(string recipient, string templateId, Dictionary<string, object> templateValues)
    {
        var document = new
        {
            id = Guid.NewGuid().ToString(),
            recipient,
            templateId,
            templateValues
        };

        var container = await GetOrCreateNotificationsContainer();
        await container.CreateItemAsync(document);
        return true;
    }

    private async Task<Container> GetOrCreateNotificationsContainer()
    {
        var database = cosmosClient.GetDatabase(options.Value.DatabaseName);
        var containerResponse = await database.CreateContainerIfNotExistsAsync("local_notifications", "/recipient");
        return containerResponse.Container;
    }
}
