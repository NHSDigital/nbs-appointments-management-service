using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    public record QueryBookingsRequest(
        [property: JsonPropertyName("from")] DateTime from,
        [property: JsonPropertyName("to")] DateTime to,
        [property: JsonPropertyName("site")] string site);
}
