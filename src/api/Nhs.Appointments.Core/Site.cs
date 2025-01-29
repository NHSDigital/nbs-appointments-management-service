using Newtonsoft.Json;

namespace Nhs.Appointments.Core;

public record Site(
    [JsonProperty("id")] string Id,
    [JsonProperty("name")] string Name,
    [JsonProperty("address")] string Address,
    [JsonProperty("phoneNumber")] string PhoneNumber,
    [JsonProperty("odsCode")] string OdsCode,
    [JsonProperty("region")] string Region,
    [JsonProperty("integratedCareBoard")] string IntegratedCareBoard,
    [JsonProperty("informationForCitizens")] string InformationForCitizens,
    [JsonProperty("accessibilities")] IEnumerable<Accessibility> Accessibilities,
    [JsonProperty("location")] Location Location
)
{
    public IEnumerable<Accessibility> AttributeValues { get; set; } = Accessibilities;
}

public record Location(
    [property: JsonProperty("type")] string Type,
    [property: JsonProperty("coordinates")]
    double[] Coordinates
);

public record Accessibility(
    [property: JsonProperty("id")] string Id,
    [property: JsonProperty("value")] string Value
);

public record SiteWithDistance(
    [JsonProperty("site")] Site Site,
    [JsonProperty("distance")] int Distance
);

public record AttributeRequest
(
    [JsonProperty("scope")]
    string Scope,
    [JsonProperty("accessibilities")]
    IEnumerable<Accessibility> Accessibilities
);

public record SitePreview
(
    [JsonProperty("id")]
    string Id,
    [JsonProperty("name")]
    string Name
);

public record DetailsRequest(
    [JsonProperty("name")] string Name,
    [JsonProperty("phoneNumber")] string PhoneNumber,
    [JsonProperty("address")] string Address,
    [JsonProperty("latitude")] string Latitude,
    [JsonProperty("longitude")] string Longitude
);
