using Newtonsoft.Json;

namespace Nhs.Appointments.ApiClient.Models
{
    public record MakeBookingRequest(
    [JsonProperty("site")]
    string Site,
    [JsonProperty("from")]
    string From,
    [JsonProperty("service")]
    string Service,
    [JsonProperty("sessionHolder")]
    string SessionHolder,
    [JsonProperty("attendeeDetails")]
    AttendeeDetails AttendeeDetails
);

}
