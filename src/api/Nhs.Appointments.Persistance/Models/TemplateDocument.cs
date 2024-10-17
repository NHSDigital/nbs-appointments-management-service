using Newtonsoft.Json;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("availability")]
public class DailyAvailabilityDocument : BookingDataCosmosDocument
{
    public DateOnly Date { get; set; }
    public ScheduleBlock[] Sessions { get; set; }
}