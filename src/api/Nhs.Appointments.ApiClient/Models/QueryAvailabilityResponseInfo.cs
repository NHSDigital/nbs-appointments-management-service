using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    public record QueryAvailabilityResponseInfo(
        [property: JsonPropertyName("date")] DateOnly date,
        [property: JsonPropertyName("blocks")] IEnumerable<QueryAvailabilityResponseBlock> blocks);
}
