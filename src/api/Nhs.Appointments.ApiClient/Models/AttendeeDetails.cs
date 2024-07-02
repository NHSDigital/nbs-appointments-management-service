using Newtonsoft.Json;

namespace Nhs.Appointments.ApiClient.Models
{
    public record AttendeeDetails(
        [JsonProperty("nhsNumber")]
    string NhsNumber,
        [JsonProperty("firstName")]
    string FirstName,
        [JsonProperty("lastName")]
    string LastName,
        [JsonProperty("dateOfBirth")]
    string DateOfBirth
    );
}
