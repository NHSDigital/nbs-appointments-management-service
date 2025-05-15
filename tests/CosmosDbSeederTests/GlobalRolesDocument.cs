using System.Text.Json.Serialization;

namespace CosmosDbSeederTests;

public class GlobalRolesDocument
{
    [JsonPropertyName("id")] public required string Id { get; set; }

    [JsonPropertyName("docType")] public required string DocType { get; set; }

    [JsonPropertyName("roles")] public required Role[] Roles { get; set; }
}

public class Role
{
    [JsonPropertyName("id")] public required string Id { get; set; }

    [JsonPropertyName("name")] public required string Name { get; set; }

    [JsonPropertyName("description")] public required string Description { get; set; }

    [JsonPropertyName("permissions")] public required string[] Permissions { get; set; }
}
