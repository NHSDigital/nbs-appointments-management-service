using System.Text.Json.Serialization;

namespace Nhs.Appointments.Core;

public class SiteConfiguration
{
    [JsonPropertyName("site")]
    public string Site { get; set; }
    
    [JsonPropertyName("informationForCitizen")]
    public string InformationForCitizen { get; set; }
    
    [JsonPropertyName("referenceNumberGroup")]
    public int ReferenceNumberGroup { get; set; }

    [JsonPropertyName("serviceConfiguration")]
    public IEnumerable<ServiceConfiguration> ServiceConfiguration { get; set; }
}

public class User
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("roleAssignments")]
    public RoleAssignment[] RoleAssignments { get; set; }
}

public class RoleAssignment
{
    [JsonPropertyName("role")]
    public string Role { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; }
}
