using Newtonsoft.Json;
using Nhs.Appointments.Core.Availability;
using System;
using System.Collections.Generic;

namespace Nhs.Appointments.Api.Models;
public record AvailabilityQueryRequest(
    [property:JsonProperty("sites", Required = Required.Always)]
    string[] Sites,
    [property:JsonProperty("attendees", Required = Required.Always)]
    List<Attendee> Attendees,
    [property:JsonProperty("from", Required = Required.Always)]
    DateOnly From,
    [property:JsonProperty("until", Required = Required.Always)]
    DateOnly Until);
