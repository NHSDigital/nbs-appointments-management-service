using Newtonsoft.Json;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("daily-site-summary-report")]
public class DailySiteSummaryDocument : AggregatedDataCosmosDocument
{
    [JsonProperty("bookings")]
    public Dictionary<string, int> Bookings { get; set; }
    [JsonProperty("cancelled")]
    public int Cancelled { get; }
    [JsonProperty("orphaned")]
    public Dictionary<string, int> Orphaned { set; get; }
    [JsonProperty("remainingCapacity")]
    public Dictionary<string, int> RemainingCapacity { set; get; }
    [JsonProperty("maximumCapacity")]
    public int MaximumCapacity { get; }
    [JsonProperty("generatedAtUtc")] 
    public DateTimeOffset GeneratedAtUtc { get; set; }
}
