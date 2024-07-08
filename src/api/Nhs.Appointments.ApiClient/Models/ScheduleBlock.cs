using Nhs.Appointments.ApiClient.Models.Converters;
using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    public class ScheduleBlock
    {
        [JsonPropertyName("from")]
        [JsonConverter(typeof(ShortTimeOnlyJsonConverter))]
        public TimeOnly From { get; set; }

        [JsonPropertyName("until")]
        [JsonConverter(typeof(ShortTimeOnlyJsonConverter))]
        public TimeOnly Until { get; set; }

        [JsonPropertyName("services")]
        public string[] Services { get; set; }

    }
}
