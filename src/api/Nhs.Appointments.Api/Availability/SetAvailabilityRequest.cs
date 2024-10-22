using Newtonsoft.Json;
using Nhs.Appointments.Core;
using System;

namespace Nhs.Appointments.Api.Availability;

public record SetAvailabilityRequest(
    [JsonProperty("date")]
    string Date,
    [JsonProperty("site")]
    string Site,
    [JsonProperty("sessions")]
    Session[] Sessions)
{
    public DateOnly AvailabilityDate => DateOnly.ParseExact(Date, "yyyy-MM-dd");
}
