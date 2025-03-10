using Nhs.Appointments.Core.Notifications;

namespace Nhs.Appointments.Core;

public interface INotificationConfigurationService
{
    Task<NotificationConfiguration> GetNotificationConfigurationsAsync(string eventType);

    Task<NotificationConfiguration> GetNotificationConfigurationsAsync(string eventType, string service);
}

