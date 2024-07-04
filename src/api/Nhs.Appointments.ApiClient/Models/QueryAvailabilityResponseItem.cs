using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    public record QueryAvailabilityResponseItem(
        [property: JsonPropertyName("site")] string site,
        [property: JsonPropertyName("service")] string service,
        [property: JsonPropertyName("availability")] List<QueryAvailabilityResponseInfo> availability);
}
