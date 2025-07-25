using Newtonsoft.Json;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("system")]
public class AggregationDocument : CoreDataCosmosDocument
{
    [JsonProperty("lastTriggerUtcDate")]
    public DateTimeOffset LastTriggeredUtcDate { get; set; }
}
