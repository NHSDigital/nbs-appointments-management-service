using Newtonsoft.Json;

namespace Nhs.Appointments.Core;

public record WellKnownOdsEntry(
    [property:JsonProperty("odsCode")]
    string OdsCode,
    [property:JsonProperty("displayName")]
    string DisplayName,
    [property:JsonProperty("type")]
    string Type
);
