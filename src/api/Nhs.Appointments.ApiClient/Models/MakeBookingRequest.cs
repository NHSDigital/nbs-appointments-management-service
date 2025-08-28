using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models;

public record MakeBookingRequest(
[property: JsonPropertyName("site")]
string Site,
[property: JsonPropertyName("from")]
string From,
[property: JsonPropertyName("service")]
string Service,
[property: JsonPropertyName("sessionHolder")]
string SessionHolder,
[property: JsonPropertyName("attendeeDetails")]
AttendeeDetails AttendeeDetails,
[property: JsonPropertyName("contactDetails")]
ContactItem[] ContactDetails
);
