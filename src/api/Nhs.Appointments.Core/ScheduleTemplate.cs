using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Nhs.Appointments.Core;

public class Session
{
    [JsonProperty("from")]
    [JsonPropertyName("from")]
    public TimeOnly From { get; set; }

    [JsonProperty("until")]
    [JsonPropertyName("until")]
    public TimeOnly Until { get; set; }

    [JsonProperty("services")]
    [JsonPropertyName("services")]
    public string[] Services { get; set; }

    [JsonProperty("slotLength")]
    [JsonPropertyName("slotLength")]
    public int SlotLength { get; set; }

    [JsonProperty("capacity")]
    [JsonPropertyName("capacity")]
    public int Capacity { get; set; }
    
}

public class SessionInstance : TimePeriod
{
    /// <summary>
    /// Not guaranteed to be generated, only used under certain paths
    /// </summary>
    internal Guid InternalSessionId { get; set; }

    public SessionInstance(TimePeriod timePeriod) : base(timePeriod.From, timePeriod.Until) { }

    public SessionInstance(DateTime from, DateTime until, bool generateSessionId = false) : base(from, until)
    {
        if (generateSessionId)
        {
            InternalSessionId = Guid.NewGuid();
        }
    }

    public string[] Services { get; set; }
    public int SlotLength { get; set; }
    public int Capacity { get; set; }
    public IEnumerable<SessionInstance> ToSlots() => Divide(TimeSpan.FromMinutes(SlotLength)).Select(sl =>
                new SessionInstance(sl) { InternalSessionId = InternalSessionId, Services = Services, Capacity = Capacity });
}

public class Template
{
    [JsonProperty("days")]
    [JsonPropertyName("days")]
    public DayOfWeek[] Days { get; set; }

    [JsonProperty("sessions")]
    [JsonPropertyName("sessions")]
    public Session[] Sessions { get; set; }
}
