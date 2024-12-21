using Newtonsoft.Json;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("system")]
public class NotificationConfigurationDocument : CoreDataCosmosDocument
{
    [JsonProperty("configs")]
    public NotificationConfigurationItem[] Configs { get; set; }
}

public class NotificationConfigurationItem
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


