using Newtonsoft.Json;
using Polly.Caching;

namespace Nhs.Appointments.Core;

public class NotificationConfiguration
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

