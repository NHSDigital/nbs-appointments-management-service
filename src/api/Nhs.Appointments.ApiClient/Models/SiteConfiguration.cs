using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    public class SiteConfiguration
    {
        [JsonPropertyName("siteId")]
        public string SiteId { get; set; }

        [JsonPropertyName("informationForCitizen")]
        public string InformationForCitizen { get; set; }

        [JsonPropertyName("referenceNumberGroup")]
        public int ReferenceNumberGroup { get; set; }

        [JsonPropertyName("serviceConfiguration")]
        public IEnumerable<ServiceConfiguration> ServiceConfiguration { get; set; }
    }
}
