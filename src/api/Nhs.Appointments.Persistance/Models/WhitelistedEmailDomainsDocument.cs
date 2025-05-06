using Newtonsoft.Json;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("system")]
public class WhitelistedEmailDomainsDocument : CoreDataCosmosDocument
{
    [JsonProperty("domains")]
    public IEnumerable<string> Domains { get; set; }
}
