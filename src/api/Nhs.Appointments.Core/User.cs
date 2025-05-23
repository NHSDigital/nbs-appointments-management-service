using Newtonsoft.Json;

namespace Nhs.Appointments.Core;

public class User
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("roleAssignments")]
    public RoleAssignment[] RoleAssignments { get; set; }

    [JsonProperty("latestAcceptedEulaVersion")]
    public DateOnly? LatestAcceptedEulaVersion { get; set; }
}

public class RoleAssignment
{
    [JsonProperty("role")]
    public string Role { get; set; }

    [JsonProperty("scope")]
    public string Scope { get; set; }
}
