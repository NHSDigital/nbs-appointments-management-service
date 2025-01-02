using Newtonsoft.Json;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("system")]
public class EulaDocument : CoreDataCosmosDocument
{
    [JsonProperty("versionDate")]
    public required DateOnly VersionDate { get; set; }
}
