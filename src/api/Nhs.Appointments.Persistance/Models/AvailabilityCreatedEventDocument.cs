using Newtonsoft.Json;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("availability_created_event")]
public class AvailabilityCreatedEventDocument : BookingDataCosmosDocument
{
    [JsonProperty("created")]
    public  DateTime Created { get; set; }

    [JsonProperty("by")]
    public  string By { get; set; }

    [JsonProperty("template")]
    public Template Template { get; set; }

    [JsonProperty("from")]
    public  DateOnly From { get; set; }

    [JsonProperty("to")]
    public DateOnly? To { get; set; }

    [JsonProperty("sessions")]
    public Session[] Sessions { get; set; }
}
