using Newtonsoft.Json;

namespace Nhs.Appointments.Api.Models;

public record SetUserRolesRequest
{
    [JsonProperty("scope")]
    public string Scope { get; set; }

    [JsonProperty("user")]
    public string User { get; set; }

    [JsonProperty("site")]
    public string Site { get; set; }

    [JsonProperty("roles")]
    public string[] Roles { get; set; }
}
