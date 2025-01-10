using Newtonsoft.Json;
using System;

namespace Nhs.Appointments.Api.Models;
public record CancelSessionRequest(
    [property:JsonProperty("site", Required = Required.Always)]
    string Site,
    [property:JsonProperty("date", Required = Required.Always)]
    DateOnly Date,
    [property:JsonProperty("from", Required = Required.Always)]
    string From,
    [property:JsonProperty("until", Required = Required.Always)]
    string Until,
    [property:JsonProperty("services", Required = Required.Always)]
    string[] Services,
    [property:JsonProperty("slotLength", Required = Required.Always)]
    int SlotLength,
    [property:JsonProperty("capacity", Required = Required.Always)]
    int Capacity);
