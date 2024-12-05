using Newtonsoft.Json;

namespace Nhs.Appointments.Core;

public class EulaVersion
{
    [JsonProperty("versionDate")]
    public required DateOnly VersionDate { get; set; }
}
