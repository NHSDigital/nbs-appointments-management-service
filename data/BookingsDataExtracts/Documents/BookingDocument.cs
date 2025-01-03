using Newtonsoft.Json;

namespace BookingsDataExtracts.Documents;

public class BookingDocument : CosmosDocument
{   
    [JsonProperty("site")]
    public string Site { get; set; }

    [JsonProperty("reference")]
    public string Reference { get; set; }

    [JsonProperty("from")]
    public DateTime From { get; set; }

    [JsonProperty("duration")]
    public int Duration { get; set; }

    [JsonProperty("service")]
    public string Service { get; set; }

    [JsonProperty("created")]
    public DateTime Created { get; set; }

    [JsonProperty("status")]
    public AppointmentStatus Status { get; set; }

    [JsonProperty("attendeeDetails")]
    public AttendeeDetails AttendeeDetails { get; set; }

    [JsonProperty("contactDetails")]
    public ContactItem[] ContactDetails { get; set; }

    [JsonProperty("additionalData")]
    public NbsAdditionalData AdditionalData { get; set; }

    [JsonProperty("reminderSent")]
    public Boolean ReminderSent { get; set; }
}

public abstract class CosmosDocument
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("docType")]
    public string DocumentType { get; set; }    
}
