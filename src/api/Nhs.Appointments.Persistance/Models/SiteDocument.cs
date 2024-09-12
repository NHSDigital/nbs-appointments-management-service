using Newtonsoft.Json;
using Nhs.Appointments.Core;
using Attribute = Nhs.Appointments.Core.Attribute;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("site")]
public class SiteDocument : IndexDataCosmosDocument
{
    [JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonProperty("address")]
    public string Address { get; set; }
    
    [JsonProperty("location")]
    public Location Location { get; set; }

    [JsonProperty("attributes")]
    public Attribute[] Attributes { get; set; }
}


