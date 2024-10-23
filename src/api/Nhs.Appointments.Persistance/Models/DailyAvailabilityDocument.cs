using Newtonsoft.Json;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("daily_availability")]
public class DailyAvailabilityDocument : BookingDataCosmosDocument
{
    [JsonProperty("date")]
    public DateOnly Date { get; set; }
    [JsonProperty("sessions")]
    public Session[] Sessions { get; set; }
}