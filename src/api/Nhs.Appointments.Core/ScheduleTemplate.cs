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

    public int SlotLength { get; set; }

    public int Capacity { get; set; }
    
}

public class SessionInstance : TimePeriod
{
    public SessionInstance(TimePeriod timePeriod) : base(timePeriod.From, timePeriod.Until) { }
    public SessionInstance(DateTime from, DateTime until) : base(from, until) { }    
    public string[] Services { get; set; }
    public int SlotLength { get; set; }
    public int Capacity { get; set; }
}

public class Schedule
{
    [JsonProperty("days")]
    [JsonPropertyName("days")]
    public DayOfWeek[] Days { get; set; }

    [JsonProperty("scheduleBlocks")]
    [JsonPropertyName("scheduleBlocks")]
    public Session[] Sessions { get; set; }
}