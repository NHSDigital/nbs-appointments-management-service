using Newtonsoft.Json;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("booking_index")]
public class BookingIndexDocument : IndexDataCosmosDocument
{
    [JsonProperty("reference")]
    public string Reference { get; set; }

    [JsonProperty("site")]
    public string Site { get; set; }

    [JsonProperty("nhsNumber")]
    public string NhsNumber { get; set; }

    [JsonProperty("from")]
    public DateTime From { get; set; }

    [JsonProperty("created")]
    public DateTimeOffset Created { get; set; }

    [JsonProperty("status")]
    public AppointmentStatus Status { get; set; }

    [JsonProperty("leadBooker")]
    public string? LeadBooker { get; set; }
}
