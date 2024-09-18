using Microsoft.Azure.Cosmos;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Persistance;

public class NotificationConfigurationStore(ITypedDocumentCosmosStore<NotificationConfiguration> cosmosStore) : INotificationConfigurationStore
{
    public Task<NotificationConfiguration> GetNotificationConfiguration(string eventType)
    {
        try
        {
            throw new NotImplementedException("cosmos query todo");
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new Exception($"Could not locate a notification configuration for event type '{eventType}'", ex);
        }
    }

    public async Task<NotificationConfiguration> GetNotificationConfigurationForService(string serviceId, string eventType)
    {
        try
        {
            throw new NotImplementedException("cosmos query todo");
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new Exception($"Could not locate a notification configuration for service type '{serviceId}' with event type '{eventType}'", ex);
        }
    }
}