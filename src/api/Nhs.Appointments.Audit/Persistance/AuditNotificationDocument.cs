using Newtonsoft.Json;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Audit.Persistance;

[CosmosDocumentType("notification")]
public class AuditNotificationDocument : AuditDataCosmosDocument
{
    [JsonProperty("destinationId")] public string DestinationId { get; set; }
    [JsonProperty("reference")] public string Reference { get; set; }
    [JsonProperty("notificationName")] public string NotificationName { get; set; }
    [JsonProperty("template")] public string Template { get; set; }
    [JsonProperty("notificationType")] public string NotificationType { get; set; }
}
