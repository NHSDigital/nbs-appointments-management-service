using Newtonsoft.Json;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("roles")]
public class RolesDocument : IndexDataCosmosDocument
{
    [JsonProperty("roles")]
    public Role[] Roles { get; set; }
}

public class Role
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("permissions")]
    public string[] Permissions { get; set; }
}