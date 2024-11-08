using System;
using Newtonsoft.Json;

namespace Nhs.Appointments.Api.Availability;

public record QueryAvailabilityRequest(
    [JsonProperty("sites")]
    string[] Sites,
    [JsonProperty("service")]
    string Service,
    [JsonProperty("from")]
    DateOnly From,
    [JsonProperty("until")]
    DateOnly Until,
    [JsonProperty("queryType")]
    QueryType QueryType
);