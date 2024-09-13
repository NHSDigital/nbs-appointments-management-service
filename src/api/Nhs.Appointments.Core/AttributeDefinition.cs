using Newtonsoft.Json;

namespace Nhs.Appointments.Core;

public record AttributeDefinition(
    [JsonProperty("id")]
    string Id,
    [JsonProperty("displayName")]
    string DisplayName
);
