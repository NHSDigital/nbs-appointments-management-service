using Microsoft.Extensions.Caching.Memory;

namespace Nhs.Appointments.Core;

public class NotificationConfigurationService(IMemoryCache memoryCache, INotificationConfigurationStore notificationConfigurationStore) : INotificationConfigurationService
{
    public async Task<NotificationConfiguration> GetNotificationConfigurationsAsync(string eventType)
    {
        var config = await LoadNotificationConfiguration();
        return config.SingleOrDefault(x => x.EventType == eventType);
    }

    public async Task<NotificationConfiguration> GetNotificationConfigurationsAsync(string eventType, string service)
    {
        var config = await LoadNotificationConfiguration();
        return config.SingleOrDefault(x => x.EventType == eventType && x.Services.Contains(service));
    }

    private async Task<IEnumerable<NotificationConfiguration>> LoadNotificationConfiguration()
    {
        const string cacheKey = "notification_configuration";
        var notificationConfiguration = memoryCache.Get<IEnumerable<NotificationConfiguration>>(cacheKey);
        if(notificationConfiguration == null)
        {
            notificationConfiguration = await notificationConfigurationStore.GetNotificationConfiguration();
            memoryCache.Set(cacheKey, notificationConfiguration, DateTimeOffset.UtcNow.AddMinutes(60));
        }
        return notificationConfiguration;
    }
}

