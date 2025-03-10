using Newtonsoft.Json;

namespace Nhs.Appointments.Api.Notifications;
public class NotificationConfiguration2
{
    [JsonProperty("services")]
    public string[] Services { get; set; }

    [JsonProperty("emailTemplate")]
    public string EmailTemplateId { get; set; }

    [JsonProperty("smsTemplate")]
    public string SmsTemplateId { get; set; }

    [JsonProperty("eventType")]
    public string EventType { get; set; }
}
