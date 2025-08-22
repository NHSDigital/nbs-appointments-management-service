using Newtonsoft.Json;
using System;

namespace Nhs.Appointments.Api.Models;

public record CancelDayRequest(
    [property:JsonProperty("site", Required = Required.Always)]
    string Site,
    [property:JsonProperty("date", Required = Required.Always)]
    DateOnly Date);
