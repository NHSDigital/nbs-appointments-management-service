using Newtonsoft.Json;
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

public record Session(
    [JsonProperty("from")]
    string From,
    [JsonProperty("until")]
    string Until,
    [JsonProperty("services")]
    string[] Services,
    [JsonProperty("slotLength")]
    int SlotLength,
    [JsonProperty("capacity")]
    int Capacity);
