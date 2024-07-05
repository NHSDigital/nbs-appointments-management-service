using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    public record CancelBookingRequest([property: JsonPropertyName("bookingReference")]string bookingReference,
        [property: JsonPropertyName("site")] string site);
}
