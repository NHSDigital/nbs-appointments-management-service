using CsvHelper.Configuration;
using Newtonsoft.Json;

namespace CsvDataTool;

public class SiteMap : ClassMap<Site>
{
    public SiteMap()
    {
        Map(m => m.Id).Name("ID");
        Map(m => m.Name).Name("Name");
        Map(m => m.Address).Name("Address");
        Map(m => m.PhoneNumber).Name("PhoneNumber");
        Map(m => m.Location).Convert(x =>
            new Location(
                "Point",
                [x.Row.GetField<double>("Longitude"), x.Row.GetField<double>("Latitude")]
                ));
        Map(m => m.IntegratedCareBoard).Name("ICB");
        Map(m => m.Region).Name("Region");
    }
}

public record struct Site(
    [JsonProperty("id")]
    string Id,
    [JsonProperty("name")]
    string Name,
    [JsonProperty("address")]
    string Address,
    [JsonProperty("phoneNumber")]
    string PhoneNumber,
    [JsonProperty("region")]
    string Region,
    [JsonProperty("integratedCareBoard")]
    string IntegratedCareBoard,
    [JsonProperty("location")]
    Location Location
)
{
}

public record struct Location
(
    [property:JsonProperty("type")]
    string Type,
    [property:JsonProperty("coordinates")]
    double[] Coordinates
);



