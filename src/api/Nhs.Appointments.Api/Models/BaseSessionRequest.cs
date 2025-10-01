using Newtonsoft.Json;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;
using System;

namespace Nhs.Appointments.Api.Models;
public record BaseSessionRequest(
    [property:JsonProperty("site", Required = Required.Always)]
    string Site,
    [property:JsonProperty("from", Required = Required.Always)]
    DateOnly From,
    [property:JsonProperty("to", Required = Required.Always)]
    DateOnly To,
    [property: JsonProperty("sessionMatcher")]
    [JsonConverter(typeof(SessionOrWildcardConverter))]
    SessionOrWildcard SessionMatcher,
    [property: JsonProperty("sessionReplacement", Required = Required.AllowNull)]
    Session? SessionReplacement);

public class SessionOrWildcard
{
    public bool IsWildcard { get; set; }
    public Session? Session { get; set; }
}
