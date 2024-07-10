using System.Text.Json.Serialization;

namespace Nhs.Appointments.Core;

public class Booking
{
    [JsonPropertyName("reference")]
    public string Reference { get; set; }

    [JsonPropertyName("from")]
    public DateTime From { get; set; }
    
    [JsonPropertyName("duration")]
    public int Duration { get; set; }
    
    [JsonPropertyName("service")]
    public string Service { get; set; }

    [JsonPropertyName("site")]
    public string Site { get; set; }

    [JsonPropertyName("sessionHolder")]
    public string SessionHolder { get; set; }
    
    [JsonPropertyName("outcome")]
    public string Outcome { get; set; }
    
    [JsonPropertyName("attendeeDetails")]
    public AttendeeDetails AttendeeDetails { get; set; }
    
    [JsonIgnore]
    public TimePeriod TimePeriod => new TimePeriod(From, TimeSpan.FromMinutes(Duration));
}

public class AttendeeDetails
{
    [JsonPropertyName("nhsNumber")]
    public string NhsNumber { get; set; }
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; }
    [JsonPropertyName("lastName")]
    public string LastName { get; set; }
    [JsonPropertyName("dateOfBirth")]
    public DateOnly DateOfBirth { get; set; }
}