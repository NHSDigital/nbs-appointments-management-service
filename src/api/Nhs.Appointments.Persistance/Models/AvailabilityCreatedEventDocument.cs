using Newtonsoft.Json;
using Nhs.Appointments.Core.Availability;

namespace Nhs.Appointments.Persistance.Models;

[CosmosDocumentType("availability_created_event")]
public class AvailabilityCreatedEventDocument : AvailabilityRecurrenceDataCosmosDocument
{
    [JsonProperty("created")]
    public required DateTime Created { get; set; }

    [JsonProperty("by")]
    public required string By { get; set; }

    [JsonProperty("template")]
    public Template Template { get; set; }

    [JsonProperty("from")]
    public required DateOnly From { get; set; }

    [JsonProperty("to")]
    public DateOnly? To { get; set; }

    [JsonProperty("sessions")]
    public Session[] Sessions { get; set; }
}
