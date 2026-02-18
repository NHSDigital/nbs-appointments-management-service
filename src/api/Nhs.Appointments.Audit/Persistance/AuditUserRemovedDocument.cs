using Newtonsoft.Json;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Audit.Persistance;

[CosmosDocumentType("userRemoved")]
public class AuditUserRemovedDocument : AuditDataCosmosDocument
{
    [JsonProperty("scope")] public string Scope { get; set; }
    [JsonProperty("removedBy")] public string RemovedBy { get; set; }
}
