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
    [property:JsonProperty("date", Required = Required.Always)]
    DateOnly Date);
