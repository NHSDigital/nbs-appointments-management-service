using Newtonsoft.Json;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Persistance.Models;

public class TypedCosmosDocument : IHaveETag
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("docType")]
    public string DocumentType { get; set; }

    [JsonProperty("_etag")]
    public string ETag { get; set; }
}

[CosmosDocument("booking_data", "site")]
public class BookingDataCosmosDocument : TypedCosmosDocument
{
    [JsonProperty("site")]
    public string Site { get; set; }
}

[CosmosDocument("index_data", "docType")]
public class IndexDataCosmosDocument : TypedCosmosDocument
{

}
