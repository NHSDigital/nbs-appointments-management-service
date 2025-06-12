using Microsoft.Extensions.Configuration;

namespace Nhs.Appointments.Api.Notifications;

public class NotificationsConfig
{
    [ConfigurationKeyName("Notifications_Provider")]
    public string NotificationsProvider { get; set; }

    public string GovNotifyBaseUri { get; set; }
    public string GovNotifyApiKey { get; set; }
}
