using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    public class TimePeriod
    {
        public TimePeriod(DateTime from, DateTime until)
        {
            if (until <= from)
            {
                throw new ArgumentException("Start must be earlier than finish");
            }

            From = from;
            Duration = until - from;
        }

        public TimePeriod(DateTime from, TimeSpan duration)
        {
            From = from;
            Duration = duration;
        }

        [JsonPropertyName("from")] public DateTime From { get; }

        [JsonPropertyName("duration")] public TimeSpan Duration { get; }

        internal DateTime Until => From.Add(Duration);
    }
}
