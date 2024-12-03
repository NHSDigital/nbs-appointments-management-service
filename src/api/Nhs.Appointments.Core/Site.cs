using Newtonsoft.Json;

namespace Nhs.Appointments.Core;

public record Site
{
    public Site() { }

    public Site(string id, string name, string address, string phoneNumber, string region, string integratedCareBoard, IEnumerable<AttributeValue> attributeValues, Location location)
    {
        Id = id;
        Name = name;
        Address = address;
        PhoneNumber = phoneNumber;
        Region = region;
        IntegratedCareBoard = integratedCareBoard;
        AttributeValues = attributeValues;
        Location = location;
    }

    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("address")]
    public string Address { get; set; }
    [JsonProperty("phoneNumber")]
    public string PhoneNumber { get; set; }
    [JsonProperty("region")]
    public string Region { get; set; }
    [JsonProperty("integratedCareBoard")]
    public string IntegratedCareBoard { get; set; }
    [JsonProperty("attributeValues")]
    public IEnumerable<AttributeValue> AttributeValues { get; set; }
    [JsonProperty("location")]
    public Location Location { get; set; }
}

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
);

public record AttributeRequest
(
    [JsonProperty("scope")]
    string Scope,
    [JsonProperty("attributeValues")]
    IEnumerable<AttributeValue> AttributeValues
);