using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    public record SetBookingStatusRequest(
        [property: JsonPropertyName("bookingReference")] string bookingReference,
        [property: JsonPropertyName("status")] string status);
}
