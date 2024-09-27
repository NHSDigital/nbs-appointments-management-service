using AutoMapper;
using Nhs.Appointments.Core;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Persistance;

public class NotificationConfigurationStore(ITypedDocumentCosmosStore<NotificationConfigurationDocument> cosmosStore, IMapper mapper) : INotificationConfigurationStore
{
    private const string DocumentId = "notification_configuration";
    public async Task<NotificationConfiguration> GetNotificationConfiguration(string eventType)
    {
        var globalDocument = await cosmosStore.GetByIdAsync<NotificationConfigurationDocument>(DocumentId);

        var items = globalDocument.Configs.Select(mapper.Map<Core.NotificationConfiguration>);

        return items.Single(config => config.EventType == eventType);
    }

    public async Task<NotificationConfiguration> GetNotificationConfigurationForService(string eventType, string serviceId)
    {
        var globalDocument = await cosmosStore.GetByIdAsync<NotificationConfigurationDocument>(DocumentId);

        var items = globalDocument.Configs.Select(mapper.Map<Core.NotificationConfiguration>);

        return items.Single(config => config.EventType == eventType && config.Services.Any(s => s == serviceId));
    }
}