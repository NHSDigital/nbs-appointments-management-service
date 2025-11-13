namespace Nhs.Appointments.Core.Messaging;

public interface INotificationConfigurationService
{
    Task<NotificationConfiguration> GetNotificationConfigurationsAsync(string eventType);

    Task<NotificationConfiguration> GetNotificationConfigurationsAsync(string eventType, string service);
}

