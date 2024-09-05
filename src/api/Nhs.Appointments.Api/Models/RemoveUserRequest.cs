using Newtonsoft.Json;

namespace Nhs.Appointments.Api.Models;

public record RemoveUserRequest
{
    [JsonProperty("user")]
    public string User { get; set; }

    [JsonProperty("site")]
    public string Site { get; set; }
}
