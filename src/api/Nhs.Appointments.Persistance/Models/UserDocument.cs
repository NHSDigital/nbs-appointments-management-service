using Newtonsoft.Json;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("user")]
public class UserDocument : CoreDataCosmosDocument
{
    [JsonProperty("apiSigningKey")]
    public string ApiSigningKey { get; set; }

    [JsonProperty("roleAssignments")]
    public RoleAssignment[] RoleAssignments { get; set; }

    [JsonProperty("latestAcceptedEulaVersion")]
    public DateOnly LatestAcceptedEulaVersion { get; set; }

    [JsonProperty("firstName")]
    public string FirstName { get; set; }

    [JsonProperty("lastName")]
    public string LastName { get; set; }
}

public class RoleAssignment
{
    [JsonProperty("role")]
    public string Role { get; set; }

    [JsonProperty("scope")]
    public string Scope { get; set; }
}
