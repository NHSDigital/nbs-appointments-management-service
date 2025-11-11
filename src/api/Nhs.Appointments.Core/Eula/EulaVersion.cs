using Newtonsoft.Json;

namespace Nhs.Appointments.Core.Eula;

public class EulaVersion
{
    [JsonProperty("versionDate")]
    public required DateOnly VersionDate { get; set; }
}
