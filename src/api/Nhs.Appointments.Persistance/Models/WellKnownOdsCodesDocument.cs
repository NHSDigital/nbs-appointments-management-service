using Newtonsoft.Json;
using Nhs.Appointments.Core.OdsCodes;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("system")]
public class WellKnownOdsCodesDocument : CoreDataCosmosDocument
{
    [JsonProperty("entries")]
    public IEnumerable<WellKnownOdsEntry> Entries { get; set; }
}
