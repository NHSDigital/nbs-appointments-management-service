using Newtonsoft.Json;

namespace Nhs.Appointments.Core;

public class EulaVersion
{
    [JsonProperty("content")]
    public required string Content { get; set; }

    [JsonProperty("versionDate")]
    public required DateOnly VersionDate { get; set; }
}
