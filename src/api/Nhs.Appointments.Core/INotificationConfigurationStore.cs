namespace Nhs.Appointments.Core;

public interface INotificationConfigurationStore
{
    Task<NotificationConfiguration> GetNotificationConfiguration(string eventType);
    Task<NotificationConfiguration> GetNotificationConfigurationForService(string serviceId, string eventType);
}
