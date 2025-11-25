using Newtonsoft.Json;
using Nhs.Appointments.Core.Availability;
using System;
using System.Collections.Generic;

namespace Nhs.Appointments.Api.Models;
public record AvailabilityQueryByHoursRequest(
    [property: JsonProperty("site", Required = Required.Always)]
    string Site,
    [property:JsonProperty("attendees", Required = Required.Always)]
    List<Attendee> Attendees,
    [property:JsonProperty("from", Required = Required.Always)]
    DateOnly From,
    [property:JsonProperty("until", Required = Required.Always)]
    DateOnly Until);
