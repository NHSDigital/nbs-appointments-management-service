using Newtonsoft.Json;

namespace Nhs.Appointments.Core;

public class SiteConfiguration : IHaveETag 
{
    [JsonProperty("site")]
    public string Site { get; set; }
    
    [JsonProperty("informationForCitizen")]
    public string InformationForCitizen { get; set; }
    
    [JsonProperty("referenceNumberGroup")]
    public int ReferenceNumberGroup { get; set; }

    [JsonProperty("serviceConfiguration")]
    public IEnumerable<ServiceConfiguration> ServiceConfiguration { get; set; }

    [JsonProperty("_etag")]
    public string ETag { get; set; }
}

