using Newtonsoft.Json;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("user_site_assignments")]
public class UserSiteAssignmentDocument : IndexDataCosmosDocument
{
    [JsonProperty("assignments")]
    public UserSiteAssignment[] Assignments { get; set; }
}

public class UserSiteAssignment
{
    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("site")]
    public string Site { get; set; }
    
    [JsonProperty("roles")]
    public string[] Roles { get; set; }
}
