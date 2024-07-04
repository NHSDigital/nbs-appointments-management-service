using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    public record QueryAvailabilityResponseBlock(
        [property: JsonPropertyName("from")]TimeOnly from, 
        [property: JsonPropertyName("until")] TimeOnly until, 
        [property: JsonPropertyName("count")] int count);
}
