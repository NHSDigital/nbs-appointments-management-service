using System;
using System.Text.Json;
using Newtonsoft.Json;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Models;

public record MakeBookingRequest(
    [property:JsonProperty("site", Required = Required.Always)]
    string Site,
    [property:JsonProperty("from", Required = Required.Always)]
    DateTime From,
    [property:JsonProperty("duration", Required = Required.Always)]
    int Duration,
    [property:JsonProperty("service", Required = Required.Always)]
    string Service,
    [property:JsonProperty("attendeeDetails", Required = Required.Always)]
    AttendeeDetails AttendeeDetails,
    [property:JsonProperty("contactDetails")]
    ContactItem[] ContactDetails,
    [property:JsonProperty("additionalData")]
    object AdditionalData,
    [property:JsonProperty("kind", Required = Required.Always)]
    BookingKind Kind
);

public enum BookingKind
{
    Booked,
    Provisional
}
