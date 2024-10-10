using AutoMapper;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class NotificationConfigurationStore(ITypedDocumentCosmosStore<NotificationConfigurationDocument> cosmosStore, IMapper mapper) : INotificationConfigurationStore
{
    private const string DocumentId = "notification_configuration";
    public async Task<NotificationConfiguration> GetNotificationConfiguration(string eventType)
    {
        var globalDocument = await GetConfig();

        var items = globalDocument.Configs.Select(mapper.Map<Core.NotificationConfiguration>);

        try
        {
            return items.Single(config => config.EventType == eventType);
        }
        catch(InvalidOperationException)
        {
            throw new Exception($"Could not retrieve notification configuration for event type '{eventType}'");
        }
    }

    public async Task<NotificationConfiguration> GetNotificationConfigurationForService(string eventType, string serviceId)
    {
        var globalDocument = await GetConfig();

        var items = globalDocument.Configs.Select(mapper.Map<Core.NotificationConfiguration>);

        try
        {
            return items.Single(config => config.EventType == eventType && config.Services.Any(s => s == serviceId));
        }
        catch (InvalidOperationException)
        {
            throw new Exception($"Could not retrieve notification configuration for event type '{eventType}' with service id '{serviceId}'");
        }
    }

    private async Task<NotificationConfigurationDocument> GetConfig()
    {
        var globalDocument = await cosmosStore.GetByIdAsync<NotificationConfigurationDocument>(DocumentId);
        if(globalDocument == null)
        {
            throw new Exception("The notification configuration document could not be retrieved");
        }

        return globalDocument;
    }
}