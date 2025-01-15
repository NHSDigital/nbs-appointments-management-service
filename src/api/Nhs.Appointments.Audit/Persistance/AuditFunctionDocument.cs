using Newtonsoft.Json;
using Nhs.Appointments.Persistance.Models;

namespace Nhs.Appointments.Audit.Persistance;

[CosmosDocumentType("function")]
public class AuditFunctionDocument : AuditDataCosmosDocument
{
    [JsonProperty("site")] public string Site { get; set; }

    [JsonProperty("functionName")] public string FunctionName { get; set; }
}
