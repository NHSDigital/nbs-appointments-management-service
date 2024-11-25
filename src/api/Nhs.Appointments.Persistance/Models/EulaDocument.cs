using Newtonsoft.Json;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("eula")]
public class EulaDocument : IndexDataCosmosDocument
{
    [JsonProperty("content")]
    public required string Content { get; set; }

    [JsonProperty("versionDate")]
    public required DateOnly VersionDate { get; set; }
}