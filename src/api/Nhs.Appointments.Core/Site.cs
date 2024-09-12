using Newtonsoft.Json;

namespace Nhs.Appointments.Core;

public record Site(
    [JsonProperty("id")]
    string Id,
    [JsonProperty("name")]
    string Name,
    [JsonProperty("address")]
    string Address,
    [JsonProperty("attributes")]
    IEnumerable<Attribute> Attributes,
    [JsonProperty("location")]
    Location Location
);

public record Location
(
    [property:JsonProperty("type")]
    string Type, 
    [property:JsonProperty("coordinates")]
    double[] Coordinates
);

public record Attribute
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
);