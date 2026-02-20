using Newtonsoft.Json;
using System;

namespace Nhs.Appointments.Api.Models;

public record ProposeCancelDateRangeRequest(
    [property: JsonProperty("site", Required = Required.Always)]
    string Site,
    [property: JsonProperty("from", Required = Required.Always)]
    DateOnly From,
    [property: JsonProperty("to", Required = Required.Always)]
    DateOnly To);
