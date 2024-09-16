using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Nhs.Appointments.Api.Models;

public record MakeBookingRequest(
    [JsonProperty("site")]
    string Site,
    [JsonProperty("from")]
    string From,
    [JsonProperty("service")]
    string Service,
    [JsonProperty("sessionHolder")]
    string SessionHolder,
    [JsonProperty("attendeeDetails")]
    AttendeeDetails AttendeeDetails
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

public record ContactDetails(
    [JsonProperty("email")]
    string Email,
    [JsonProperty("phoneNumber")]
    string PhoneNumber,
    [JsonProperty("emailContactConsent")]
    bool EmailContactConsent,
    [JsonProperty("phoneContactConsent")]
    bool PhoneContactConsent
    )
{ }
