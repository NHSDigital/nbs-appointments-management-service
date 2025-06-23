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

public class SessionInstance(DateTime from, DateTime until) : TimePeriod(from, until)
{
    public SessionInstance(TimePeriod timePeriod) : this(timePeriod.From, timePeriod.Until) { }

    public string[] Services { get; set; }
    public int SlotLength { get; set; }
    public int Capacity { get; set; }
    public virtual IEnumerable<SessionInstance> ToSlots() => Divide(TimeSpan.FromMinutes(SlotLength)).Select(sl =>
                new SessionInstance(sl) { Services = Services, Capacity = Capacity });
}

/// <summary>
/// A session instance that has the capability to generate an internal sessionId to help link slots with sessions
/// </summary>
public class LinkedSessionInstance : SessionInstance
{
    private LinkedSessionInstance(TimePeriod timePeriod) : base(timePeriod) { }
    
    public LinkedSessionInstance(DateTime from, DateTime until) : base(from, until) { }
    
    public LinkedSessionInstance(SessionInstance sessionInstance, bool generateSessionId) : base(sessionInstance)
    {
        if (generateSessionId)
        {
            InternalSessionId = Guid.NewGuid();
        }
        
        Services = sessionInstance.Services;
        Capacity = sessionInstance.Capacity;
        SlotLength = sessionInstance.SlotLength;
    }
    
    public Guid? InternalSessionId { get; init; }
    
    public override IEnumerable<LinkedSessionInstance> ToSlots() => Divide(TimeSpan.FromMinutes(SlotLength)).Select(slot =>
        new LinkedSessionInstance(slot)
        {
            InternalSessionId = InternalSessionId, 
            Services = Services, 
            Capacity = Capacity, 
            SlotLength = SlotLength
        });
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
