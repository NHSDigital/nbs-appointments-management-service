using Newtonsoft.Json;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("site_configuration")]
public class SiteConfigurationDocument : IndexDataCosmosDocument
{        
    [JsonProperty("siteId")]
    public string SiteId{ get; set; }

    [JsonProperty("siteName")]
    public string SiteName { get; set; }

    [JsonProperty("informationForCitizen")]
    public string InformationForCitizen { get; set; }

    [JsonProperty("referenceNumberGroup")]
    public int ReferenceNumberGroup { get; set; }  

    [JsonProperty("serviceConfiguration")]
    public IEnumerable<ServiceConfiguration> ServiceConfiguration{ get; set; }
}
