using System.Text.Json.Serialization;

namespace Nhs.Appointments.ApiClient.Models
{
    public record AttendeeDetails(
        [property: JsonPropertyName("nhsNumber")]
    string NhsNumber,
        [property: JsonPropertyName("firstName")]
    string FirstName,
        [property: JsonPropertyName("lastName")]
    string LastName,
        [property: JsonPropertyName("dateOfBirth")]
    string DateOfBirth
    );
}
