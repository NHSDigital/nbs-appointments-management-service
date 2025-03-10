using Microsoft.Extensions.Options;
using Nhs.Appointments.Core.Notifications;

namespace Nhs.Appointments.Core;

public class NotificationConfigurationService : INotificationConfigurationService
{
    private readonly List<NotificationConfiguration> _configurations;

    public NotificationConfigurationService(IOptions<CommsOptions> commsOptions)
    {
        _configurations = commsOptions.Value?.Configurations ?? new List<NotificationConfiguration>();
    }

    public async Task<NotificationConfiguration> GetNotificationConfigurationsAsync(string eventType)
    {
        return _configurations.SingleOrDefault(x => x.EventType == eventType);
    }

    public async Task<NotificationConfiguration> GetNotificationConfigurationsAsync(string eventType, string service)
    {
        //return _configurations.SingleOrDefault(x => x.EventType == eventType && x.Services.Contains(service));
        return _configurations.SingleOrDefault(x => x.EventType == eventType);
    }
}

