using Newtonsoft.Json;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Audit.Persistance;

[CosmosDocumentType("function")]
public class AuditFunctionDocument : AuditDataCosmosDocument
{
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonProperty("siteId")]
    public string SiteId { get; set; }

    [JsonProperty("userId")]
    public string UserId { get; set; }

    [JsonProperty("actionType")]
    public string ActionType { get; set; }
}
