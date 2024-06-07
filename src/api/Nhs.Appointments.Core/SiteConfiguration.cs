using Newtonsoft.Json;

namespace Nhs.Appointments.Core;

public class SiteConfiguration
{
    [JsonProperty("siteId")]
    public string SiteId { get; set; }
    
    [JsonProperty("informationForCitizen")]
    public string InformationForCitizen { get; set; }
    
    [JsonProperty("referenceNumberGroup")]
    public int ReferenceNumberGroup { get; set; }

    [JsonProperty("serviceConfiguration")]
    public IEnumerable<ServiceConfiguration> ServiceConfiguration { get; set; }
}

public class UserAssignment
{
    public string Email { get; set; }
    public string Site { get; set; }
    public string[] Roles { get; set; }
}
