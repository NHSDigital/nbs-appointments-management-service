using Newtonsoft.Json;

namespace Nhs.Appointments.ApiClient.Models
{
    public record ServiceConfiguration(
    [JsonProperty("code")] string Code,
    [JsonProperty("displayName")] string DisplayName,
    [JsonProperty("duration")] int Duration,
    [JsonProperty("enabled")] bool Enabled);
}
