using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    public record Site(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("name")] string Name,
        [property: JsonPropertyName("address")] string Address);
}
