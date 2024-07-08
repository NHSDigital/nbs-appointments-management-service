using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    public class SessionInstance : TimePeriod
    {
        public SessionInstance(TimePeriod timePeriod) : base(timePeriod.From, timePeriod.Until) { }
        public SessionInstance(DateTime from, DateTime until) : base(from, until) { }
        
        [JsonPropertyName("sessionHolder")] 
        public string SessionHolder { get; set; }
    }
}
