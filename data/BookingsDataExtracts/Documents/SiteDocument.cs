using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace BookingsDataExtracts.Documents;

public class SiteDocument : CosmosDocument
{ 
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("phoneNumber")]
    public string PhoneNumber { get; set; }

    [JsonProperty("region")]
    public string Region { get; set; }

    [JsonProperty("integratedCareBoard")]
    public string IntegratedCareBoard { get; set; }

    [JsonProperty("location")]
    public Location Location { get; set; }    
}
