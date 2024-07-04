using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    public record CancelBookingResponse([property: JsonPropertyName("bookingReference")]string BookingReference, [property: JsonPropertyName("status")] string Status);
}
