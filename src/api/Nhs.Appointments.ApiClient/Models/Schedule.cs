using System.Text.Json.Serialization;
using Nhs.Appointments.ApiClient.Models.Converters;

namespace Nhs.Appointments.ApiClient.Models
{
    public class Schedule
    {
        [JsonPropertyName("days")]
        [JsonConverter(typeof(DayOfWeekListJsonConverter))]
        public DayOfWeek[] Days { get; set; }

        [JsonPropertyName("scheduleBlocks")]
        public ScheduleBlock[] ScheduleBlocks { get; set; }
    }
}
