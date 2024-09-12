using Newtonsoft.Json;

namespace Nhs.Appointments.Core;

public record AttributeSets(
    [JsonProperty("id")]
    string Id,
    [JsonProperty("sets")]
    AttributeSet[] Sets
);

public record AttributeSet(
    [JsonProperty("id")]
    string Id,
    [JsonProperty("attributeDefinitions")]
    AttributeDefinition[] AttributeDefinitions
);

public record AttributeDefinition(
    [JsonProperty("id")]
    string Id,
    [JsonProperty("displayName")]
    string DisplayName
);
