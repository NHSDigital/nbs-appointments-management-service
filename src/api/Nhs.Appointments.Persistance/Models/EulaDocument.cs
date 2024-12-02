using Newtonsoft.Json;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("eula")]
public class EulaDocument : IndexDataCosmosDocument
{
    [JsonProperty("versionDate")]
    public required DateOnly VersionDate { get; set; }
}