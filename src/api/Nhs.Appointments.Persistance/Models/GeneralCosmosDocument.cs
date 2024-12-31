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

[CosmosDocument("site_data", "docType")]
public class SiteDataCosmosDocument : TypedCosmosDocument
{

}

[CosmosDocument("core_data", "docType")]
public class CoreDataCosmosDocument : TypedCosmosDocument
{

}

[CosmosDocument("index_data", "docType")]
public class IndexDataCosmosDocument : TypedCosmosDocument
{

}
