using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

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
    [JsonProperty("location")] Location Location,
    [property: JsonProperty("status")] SiteStatus? status
)
{
    public IEnumerable<Accessibility> Accessibilities { get; set; } = Accessibilities;
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

/// <summary>
/// Filter sites based on whether they support the provided service within the date range
/// </summary>
public record SiteSupportsServiceFilter(
    string service,
    DateOnly from,
    DateOnly until
);

public record AccessibilityRequest
(
    [JsonProperty("accessibilities")]
    IEnumerable<Accessibility> Accessibilities
);

public record InformationForCitizensRequest
(
    [JsonProperty("informationForCitizens")]
    string InformationForCitizens
);

public record SitePreview
(
    [JsonProperty("id")]
    string Id,
    [JsonProperty("name")]
    string Name,
    [JsonProperty("odsCode")]
    string OdsCode,
    [JsonProperty("integratedCareBoard")]
    string IntegratedCareBoard
);

public record DetailsRequest(
    [JsonProperty("name")] string Name,
    [JsonProperty("phoneNumber")] string PhoneNumber,
    [JsonProperty("address")] string Address,
    [JsonProperty("longitude")] string Longitude,
    [JsonProperty("latitude")] string Latitude
);

public record ReferenceDetailsRequest(
    [JsonProperty("odsCode")] string OdsCode,
    [JsonProperty("icb")] string Icb,
    [JsonProperty("region")] string Region
);

[JsonConverter(typeof(StringEnumConverter))]
public enum SiteStatus
{
    [EnumMember(Value = "Online")]
    Online,
    [EnumMember(Value = "Offline")]
    Offline
}
