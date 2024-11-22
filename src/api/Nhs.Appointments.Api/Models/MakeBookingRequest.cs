using System;
using System.Globalization;
using System.Text.Json;
using Newtonsoft.Json;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Models;

public record MakeBookingRequest(
    [JsonProperty("site")]
    string Site,
    [JsonProperty("from")]
    string From,
    [JsonProperty("duration")]
    int Duration,
    [JsonProperty("service")]
    string Service,
    [JsonProperty("attendeeDetails")]
    AttendeeDetails AttendeeDetails,
    [JsonProperty("contactDetails")]
    ContactItem[] ContactDetails,
    [JsonProperty("additionalData")]
    object? AdditionalData,
    [JsonProperty("provisional")]
    bool Provisional = false
)

{
    public DateTime FromDateTime => DateTime.ParseExact(From, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

};