using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    public record MakeBookingResponse([property: JsonPropertyName("bookingReference")]string BookingReference);
}
