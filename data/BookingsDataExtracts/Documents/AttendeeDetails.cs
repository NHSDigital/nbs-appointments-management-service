using Newtonsoft.Json;

namespace BookingsDataExtracts.Documents;

public class AttendeeDetails
{
    [JsonProperty("nhsNumber")]
    public string NhsNumber { get; set; }
    [JsonProperty("firstName")]
    public string FirstName { get; set; }
    [JsonProperty("lastName")]
    public string LastName { get; set; }
    [JsonProperty("dateOfBirth")]
    public DateOnly DateOfBirth { get; set; }
}
