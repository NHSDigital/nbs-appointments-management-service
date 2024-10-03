using Newtonsoft.Json;

namespace Nhs.Appointments.Core;

public record Site (
    [JsonProperty("id")]
    string Id,
    [JsonProperty("name")]
    string Name,
    [JsonProperty("address")]
    string Address,
    [JsonProperty("attributeValues")]
    IEnumerable<AttributeValue> AttributeValues,
    [JsonProperty("location")]
    Location Location
) : DomainRoot;

public record Location
(
    [property:JsonProperty("type")]
    string Type, 
    [property:JsonProperty("coordinates")]
    double[] Coordinates
);

public record AttributeValue
(
    [property:JsonProperty("id")]
    string Id, 
    [property:JsonProperty("value")]
    string Value
);

public record SiteWithDistance
(
    [JsonProperty("site")]
    Site Site,
    [JsonProperty("distance")]
    int Distance 
) : DomainRoot;