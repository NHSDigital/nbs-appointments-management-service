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
    [JsonProperty("attributeValues")] IEnumerable<AttributeValue> AttributeValues,
    [JsonProperty("location")] Location Location
)
{
    public IEnumerable<AttributeValue> AttributeValues { get; set; } = AttributeValues;
}

public record Location(
    [property: JsonProperty("type")] string Type,
    [property: JsonProperty("coordinates")]
    double[] Coordinates
);

public record AttributeValue(
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
    [JsonProperty("attributeValues")]
    IEnumerable<AttributeValue> AttributeValues
);

public record SitePreview
(
    [JsonProperty("id")]
    string Id,
    [JsonProperty("name")]
    string Name
);
