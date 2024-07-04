using Newtonsoft.Json;

namespace Nhs.Appointments.Core;

public class SiteConfiguration
{
    [JsonProperty("site")]
    public string Site { get; set; }
    
    [JsonProperty("informationForCitizen")]
    public string InformationForCitizen { get; set; }
    
    [JsonProperty("referenceNumberGroup")]
    public int ReferenceNumberGroup { get; set; }

    [JsonProperty("serviceConfiguration")]
    public IEnumerable<ServiceConfiguration> ServiceConfiguration { get; set; }
}

public class User
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("roleAssignments")]
    public RoleAssignment[] RoleAssignments { get; set; }
}

public class RoleAssignment
{
    [JsonProperty("role")]
    public string Role { get; set; }

    [JsonProperty("scope")]
    public string Scope { get; set; }
}
