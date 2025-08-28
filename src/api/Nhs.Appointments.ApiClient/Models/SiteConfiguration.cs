using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    public class SiteConfiguration
    {
        [JsonPropertyName("site")]
        public string Site { get; set; }

        [JsonPropertyName("informationForCitizen")]
        public string InformationForCitizen { get; set; }

        [JsonPropertyName("referenceNumberGroup")]
        public int ReferenceNumberGroup { get; set; }

        [JsonPropertyName("serviceConfiguration")]
        public IEnumerable<ServiceConfiguration> ServiceConfiguration { get; set; }
    }
}
