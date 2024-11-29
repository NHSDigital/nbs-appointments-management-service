using System;
using Newtonsoft.Json;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Availability;

public record ApplyAvailabilityTemplateRequest(
    [property:JsonProperty("site", Required = Required.Always)]
    string Site,
    [property:JsonProperty("from", Required = Required.Always)]
    DateOnly From,
    [property:JsonProperty("until", Required = Required.Always)]
    DateOnly Until,
    [property:JsonProperty("template", Required = Required.Always)]
    Template Template,
    [property:JsonProperty("mode", Required = Required.Always)]
    ApplyAvailabilityMode Mode
);