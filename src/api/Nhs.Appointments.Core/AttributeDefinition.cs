using Newtonsoft.Json;

namespace Nhs.Appointments.Core;

public record AttributeDefinition(
    [property:JsonProperty("id")]
    string Id,
    [property:JsonProperty("displayName")]
    string DisplayName
);
