using System.Text.Json.Serialization;

namespace CosmosDbSeederTests;

public class NotificationConfigDocument
{
    [JsonPropertyName("id")] public required string Id { get; set; }

    [JsonPropertyName("docType")] public required string DocType { get; set; }

    [JsonPropertyName("configs")] public required Config[] Configs { get; set; }
}

public class Config
{
    [JsonPropertyName("services")] public required string[] Services { get; set; }

    [JsonPropertyName("emailTemplate")] public required string EmailTemplate { get; set; }

    [JsonPropertyName("smsTemplate")] public required string SmsTemplate { get; set; }

    [JsonPropertyName("eventType")] public required string EventType { get; set; }
}
