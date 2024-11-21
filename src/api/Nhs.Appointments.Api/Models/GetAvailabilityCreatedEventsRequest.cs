using System;
using Newtonsoft.Json;

namespace Nhs.Appointments.Api.Models;
public record GetAvailabilityCreatedEventsRequest(
    [JsonProperty("site")]
    string Site,
    [JsonProperty("from")]
    string From
)

{
    public DateOnly FromDate => DateOnly.ParseExact(From, "yyyy-MM-dd");
};