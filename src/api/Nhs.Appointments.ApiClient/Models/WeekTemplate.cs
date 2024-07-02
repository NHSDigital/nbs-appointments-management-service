using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    public class WeekTemplate
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonProperty("site")]
        [JsonPropertyName("site")]
        public string Site { get; set; }

        [JsonProperty("items")]
        [JsonPropertyName("items")]
        public Schedule[] Items { get; set; }
    }
}
