using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Nhs.Appointments.Core.Helpers;

namespace Nhs.Appointments.Core.Availability;

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

public class SessionInstance(DateTime from, DateTime until) : TimePeriod(from, until), IEquatable<SessionInstance>
{
    public SessionInstance(TimePeriod timePeriod) : this(timePeriod.From, timePeriod.Until) { }

    public string[] Services { get; set; }
    public int SlotLength { get; set; }
    public int Capacity { get; set; }
    public virtual IEnumerable<SessionInstance> ToSlots() => Divide(TimeSpan.FromMinutes(SlotLength)).Select(sl =>
                new SessionInstance(sl) { Services = Services, Capacity = Capacity });
    
    public bool Equals(SessionInstance other)
    {
        if (other is null)
        {
            return false;
        }

        return From == other.From &&
               Until == other.Until &&
               SlotLength == other.SlotLength &&
               Capacity == other.Capacity &&
               Services.SequenceEqual(other.Services);
    }
    
    public override bool Equals(object obj) => Equals(obj as SessionInstance);

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = (hash * 23) + From.GetHashCode();
            hash = (hash * 23) + Until.GetHashCode();
            hash = (hash * 23) + SlotLength.GetHashCode();
            hash = (hash * 23) + Capacity.GetHashCode();

            if (Services != null)
            {
                foreach (var service in Services)
                {
                    hash = (hash * 23) + (service?.GetHashCode() ?? 0);
                }
            }

            return hash;
        }
    }
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

public enum SessionUpdateAction
{
    CancelMultiple,
    EditMultiple,
    EditSingle,
    CancelSingle
}
