﻿using System;
using Newtonsoft.Json;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Availability;

public record SetAvailabilityRequest(
    [property:JsonProperty("date", Required = Required.Always)]
    DateOnly Date,
    [property:JsonProperty("site", Required = Required.Always)]
    string Site,
    [property:JsonProperty("sessions", Required = Required.Always)]
    Session[] Sessions,
    [property:JsonProperty("mode", Required = Required.Always)]
    ApplyAvailabilityMode Mode,
    [property: JsonProperty("sessionToEdit", Required = Required.Default)]
    Session SessionToEdit = null
);
