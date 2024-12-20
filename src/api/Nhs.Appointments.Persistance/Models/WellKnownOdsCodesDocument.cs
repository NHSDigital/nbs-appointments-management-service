using Newtonsoft.Json;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("system")]
public class WellKnownOdsCodesDocument : IndexDataCosmosDocument
{
    [JsonProperty("entries")]
    public IEnumerable<WellKnownOdsEntry> Entries { get; set; }
}
