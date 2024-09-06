using Newtonsoft.Json;

namespace Nhs.Appointments.Core;

public record Site(
    [JsonProperty("id")]
    string Id,
    [JsonProperty("name")]
    string Name,
    [JsonProperty("address")]
    string Address,
    [JsonProperty("location")]
    Location? Location = null
);

public record Location
(
    [JsonProperty("type")]
    string Type, 
    [JsonProperty("coordinates")]
    double[] Coordinates
);

public record SiteWithDistance
(
    [JsonProperty("site")]
    Site Site,
    [JsonProperty("distance")]
    double Distance 
);