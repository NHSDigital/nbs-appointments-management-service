using Newtonsoft.Json;

namespace Nhs.Appointments.Persistance.Models;

public class TypedCosmosDocument
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("docType")]
    public string DocumentType { get; set; }
}

[CosmosDocument("booking_data", "site")]
public class BookingDataCosmosDocument : TypedCosmosDocument
{
    [JsonProperty("site")]
    public string Site { get; set; }
}

[CosmosDocument("core_data", "docType")]
public class CoreDataCosmosDocument : TypedCosmosDocument
{

}

[CosmosDocument("aggregated_data", "date")]
public class AggregatedDataCosmosDocument : TypedCosmosDocument
{
    [JsonProperty("date")]
    public DateOnly Date { get; set; }
}


[CosmosDocument("index_data", "docType")]
public class IndexDataCosmosDocument : TypedCosmosDocument
{

}
