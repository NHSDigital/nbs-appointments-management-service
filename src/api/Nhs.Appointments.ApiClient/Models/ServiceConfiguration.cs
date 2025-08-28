using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    public record ServiceConfiguration(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("displayName")] string DisplayName,
    [property: JsonPropertyName("duration")] int Duration,
    [property: JsonPropertyName("enabled")] bool Enabled);
}
