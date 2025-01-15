using Newtonsoft.Json;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Audit.Persistance;

[CosmosDocument("audit_data", "user")]
public class AuditDataCosmosDocument : TypedCosmosDocument
{
    [JsonProperty("user")] public string User { get; set; }

    [JsonProperty("timestamp")] public DateTime Timestamp { get; set; }
}
