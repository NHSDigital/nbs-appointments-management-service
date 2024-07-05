using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    public class WeekTemplate
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("site")]
        public string Site { get; set; }

        [JsonPropertyName("items")]
        public Schedule[] Items { get; set; }
    }
}
