using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Nhs.Appointments.Core.Geography;

namespace Nhs.Appointments.Core.Sites;

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
    [JsonProperty("location")] Location location,
    [JsonProperty("status")] SiteStatus? status,
    [JsonProperty("isDeleted")] bool? isDeleted,
    [JsonProperty("type")] string Type
)
{
    public IEnumerable<Accessibility> Accessibilities { get; set; } = Accessibilities?.Select(a => new Accessibility(a.Id, a.Value.ToLower()));

    private Location Location { get; } = location;

    public Coordinates Coordinates
    {
        get
        {
            return Location?.Coordinates is not { Length: 2 }
                ? null
                : new Coordinates { Longitude = Location.Coordinates[0], Latitude = Location.Coordinates[1] };
        }
    }
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
    List<string> services,
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
