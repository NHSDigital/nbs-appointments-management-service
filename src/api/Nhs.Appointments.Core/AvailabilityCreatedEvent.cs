using Newtonsoft.Json;

namespace Nhs.Appointments.Core;

public class AvailabilityCreatedEvent
{
    [JsonProperty("created")]
    public required DateTime Created { get; set; }

    [JsonProperty("by")]
    public required string By { get; set; }

    [JsonProperty("template")]
    public Template? Template { get; set; }

    [JsonProperty("from")]
    public required DateOnly From { get; set; }

    [JsonProperty("to")]
    public DateOnly? To { get; set; }

    [JsonProperty("sessions")]
    public Session[]? Sessions { get; set; }
}