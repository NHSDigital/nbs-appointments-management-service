using System;
using Newtonsoft.Json;

namespace Nhs.Appointments.Api.Availability;

public record QueryAvailabilityRequest(
    [JsonProperty("sites")]
    string[] Sites,
    [JsonProperty("service")]
    string Service,
    [JsonProperty("from")]
    string From,
    [JsonProperty("until")]
    string Until,
    [JsonProperty("queryType")]
    QueryType QueryType
)

{
    public DateOnly FromDate => DateOnly.ParseExact(From, "yyyy-MM-dd");
    public DateOnly UntilDate => DateOnly.ParseExact(Until, "yyyy-MM-dd");
}
