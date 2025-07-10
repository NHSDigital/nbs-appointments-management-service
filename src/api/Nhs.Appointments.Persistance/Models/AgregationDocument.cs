using Newtonsoft.Json;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("aggregation")]
public class AggregationDocument : CoreDataCosmosDocument
{
    [JsonProperty("lastTriggerUtcDate")]
    public DateTimeOffset LastTriggeredUtcDate { get; set; }
}
