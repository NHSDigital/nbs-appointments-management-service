using Newtonsoft.Json;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("system")]
public class AggregationDocument : CoreDataCosmosDocument
{
    [JsonProperty("lastTriggerUtcDate")]
    public DateTimeOffset LastTriggeredUtcDate { get; set; }

    [JsonProperty("lastRunMetaData")]
    public AggregationLastRunMetaData LastRunMetaData { get; set; }
}

public class AggregationLastRunMetaData
{
    [JsonProperty("fromDateOnly")]
    public DateOnly FromDateOnly { get; set; }
    [JsonProperty("toDateOnly")]
    public DateOnly ToDateOnly { get; set; }
    [JsonProperty("lastRanToDateOnly")]
    public DateOnly LastRanToDateOnly { get; set; }
}
