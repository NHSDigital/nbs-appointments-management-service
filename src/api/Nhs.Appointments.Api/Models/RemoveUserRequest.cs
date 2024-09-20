using Newtonsoft.Json;

namespace Nhs.Appointments.Api.Models;

public record RemoveUserRequest
{
    [JsonProperty("user")]
    public string User { get; init; }

    [JsonProperty("site")]
    public string Site { get; init; }
}
