using System;
using System.Globalization;
using Newtonsoft.Json;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Models;

public record ApplyAvailabilityTemplateRequest(
    [JsonProperty("site")]
    string Site,
    [JsonProperty("from")]
    string From,
    [JsonProperty("until")]
    string Until,
    [JsonProperty("template")]
    Template Template
)
{
    public DateOnly FromDate => DateOnly.ParseExact(From, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    public DateOnly UntilDate => DateOnly.ParseExact(Until, "yyyy-MM-dd",  CultureInfo.InvariantCulture);
};