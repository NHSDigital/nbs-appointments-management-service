using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Nhs.Appointments.Api.Auth;

public class TokenResponse
{
    [JsonProperty("id_token")]
    public string IdToken { get; set; }
}
