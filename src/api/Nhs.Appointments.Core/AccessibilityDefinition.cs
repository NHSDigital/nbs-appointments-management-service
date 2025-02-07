using Newtonsoft.Json;

namespace Nhs.Appointments.Core;

public record AccessibilityDefinition(
    [property:JsonProperty("id")]
    string Id,
    [property:JsonProperty("displayName")]
    string DisplayName
);
