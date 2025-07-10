using System;
using Newtonsoft.Json;

namespace Nhs.Appointments.Api.Availability;

public record HasAnyAvailableSlotRequest(
    [JsonProperty("sites")]
    string[] Sites,
    [JsonProperty("service")]
    string Service,
    [JsonProperty("from")]
    DateOnly From,
    [JsonProperty("until")]
    DateOnly Until
);
