using Newtonsoft.Json;
using Nhs.Appointments.Core;
using System.Text.Json;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("booking")]
public class BookingDocument : BookingDataCosmosDocument
{
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
    public object AdditionalData { get; set; }
}
