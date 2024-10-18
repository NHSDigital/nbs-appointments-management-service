using Newtonsoft.Json;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("availability")]
public class DailyAvailabilityDocument : BookingDataCosmosDocument
{
    [JsonProperty("date")]
    public DateOnly Date { get; set; }
    [JsonProperty("sessions")]
    public ScheduleBlock[] Sessions { get; set; }
}