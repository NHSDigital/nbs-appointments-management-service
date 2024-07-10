using System.Text.Json.Serialization;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("apiClient")]
public class ApiClientProfileDocument : ApiClientProfileCosmosDocument
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("signingKey")]
    public string SigningKey { get; set; }

    [JsonPropertyName("roleAssignments")]
    public IEnumerable<RoleAssignment> RoleAssignments { get; set; }

    public class RoleAssignment
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }
    }
}
