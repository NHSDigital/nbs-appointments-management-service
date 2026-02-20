using Newtonsoft.Json;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Audit.Persistance;

[CosmosDocumentType("userRemoved")]
public class AuditUserRemovedDocument : AuditDataCosmosDocument
{
    [JsonProperty("scope")] public string Scope { get; set; }
    [JsonProperty("removedUser")] public string RemovedUser { get; set; }
}
