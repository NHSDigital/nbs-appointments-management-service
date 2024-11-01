using System;
using System.Globalization;
using Newtonsoft.Json;

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
    [JsonProperty("provisional")]
    bool Provisional = false
)

{
    public DateTime FromDateTime => DateTime.ParseExact(From, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
};

public record AttendeeDetails(
    [JsonProperty("nhsNumber")]
    string NhsNumber,
    [JsonProperty("firstName")]
    string FirstName,
    [JsonProperty("lastName")]
    string LastName,
    [JsonProperty("dateOfBirth")]
    string DateOfBirth
)

{
    public DateOnly BirthDate => DateOnly.ParseExact(DateOfBirth, "yyyy-MM-dd");
};

public record ContactItem(
    [JsonProperty("type")]
    string Type,
    [JsonProperty("value")]
    string Value
    ) { }
