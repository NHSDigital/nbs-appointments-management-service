using System;
using System.Globalization;
using Newtonsoft.Json;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Models;

public record ApplyAvailabilityTemplateRequest(
    [JsonProperty("site")]
    string Site,
    [JsonProperty("from")]
    DateOnly From,
    [JsonProperty("until")]
    DateOnly Until,
    [JsonProperty("template")]
    Template Template
);