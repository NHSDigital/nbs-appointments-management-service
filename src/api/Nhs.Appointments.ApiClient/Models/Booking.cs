using Newtonsoft.Json;

namespace Nhs.Appointments.ApiClient.Models
{
    public class Booking
    {
        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("from")]
        public DateTime From { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("service")]
        public string Service { get; set; }

        [JsonProperty("site")]
        public string Site { get; set; }

        [JsonProperty("sessionHolder")]
        public string SessionHolder { get; set; }

        [JsonProperty("outcome")]
        public string Outcome { get; set; }

        [JsonProperty("attendeeDetails")]
        public AttendeeDetails AttendeeDetails { get; set; }

        [JsonIgnore]
        public TimePeriod TimePeriod => new TimePeriod(From, TimeSpan.FromMinutes(Duration));
    }
}
