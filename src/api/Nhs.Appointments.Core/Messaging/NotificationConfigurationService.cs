using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Core.Caching;

namespace Nhs.Appointments.Core.Messaging;

public class NotificationConfigurationService(ICacheService cacheService, INotificationConfigurationStore notificationConfigurationStore) : INotificationConfigurationService
{
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(60);
    
    public async Task<NotificationConfiguration> GetNotificationConfigurationsAsync(string eventType)
    {
        var config = await LoadNotificationConfiguration();
        return config.SingleOrDefault(x => x.EventType == eventType);
    }

    public async Task<NotificationConfiguration> GetNotificationConfigurationsAsync(string eventType, string service)
    {
        var config = await LoadNotificationConfiguration();

        var matchingConfigs = config
            .Where(x => x.EventType == eventType && x.Services.Contains(service))
            .ToList();

        if (matchingConfigs.Count == 0)
            return null;

        var combinedConfig = new NotificationConfiguration
        {
            EventType = eventType,
            Services = [ service ],
            EmailTemplateId = matchingConfigs.Select(x => x.EmailTemplateId).FirstOrDefault(t => !string.IsNullOrWhiteSpace(t)),
            SmsTemplateId = matchingConfigs.Select(x => x.SmsTemplateId).FirstOrDefault(t => !string.IsNullOrWhiteSpace(t))
        };

        return combinedConfig;
    }

    private async Task<IEnumerable<NotificationConfiguration>> LoadNotificationConfiguration()
    {
        return await cacheService.GetCacheValue(
            CacheKeys.NotificationConfiguration,
            new CacheOptions<IEnumerable<NotificationConfiguration>>(
                notificationConfigurationStore.GetNotificationConfiguration,
                _cacheDuration));
    }
}

