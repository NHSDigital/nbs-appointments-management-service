using Newtonsoft.Json;

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
    public DateTime Created { get; set; }

    [JsonProperty("provisional")]
    public bool Provisional { get; set; }
}
