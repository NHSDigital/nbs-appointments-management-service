using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Nhs.Appointments.Core;

public class ScheduleBlock
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
    
}

public class SessionInstance : TimePeriod
{
    public SessionInstance(TimePeriod timePeriod) : base(timePeriod.From, timePeriod.Until) { }
    public SessionInstance(DateTime from, DateTime until) : base(from, until) { }
    public string SessionHolder { get; set; }
}

public class WeekTemplate : IHaveETag
{
    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonProperty("site")]
    [JsonPropertyName("site")]
    public string Site { get; set; }

    [JsonProperty("items")]
    [JsonPropertyName("items")]
    public Schedule[] Items { get; set; }

    [JsonProperty("_etag")]
    public string ETag { get; set; }
}

public class Schedule
{
    [JsonProperty("days")]
    [JsonPropertyName("days")]
    public DayOfWeek[] Days { get; set; }

    [JsonProperty("scheduleBlocks")]
    [JsonPropertyName("scheduleBlocks")]
    public ScheduleBlock[] ScheduleBlocks { get; set; }
}

public class TemplateAssignment
{    
    public DateOnly From { get; set; }
    public DateOnly Until { get; set; }
    public string TemplateId { get; set; }  
}