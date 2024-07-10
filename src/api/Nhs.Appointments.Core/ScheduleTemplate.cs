using System.Text.Json.Serialization;

namespace Nhs.Appointments.Core;

public class ScheduleBlock
{
    [JsonPropertyName("from")]
    public TimeOnly From { get; set; }

    [JsonPropertyName("until")]
    public TimeOnly Until { get; set; }

    [JsonPropertyName("services")]
    public string[] Services { get; set; }
    
}

public class SessionInstance : TimePeriod
{
    public SessionInstance(TimePeriod timePeriod) : base(timePeriod.From, timePeriod.Until) { }
    public SessionInstance(DateTime from, DateTime until) : base(from, until) { }
    public string SessionHolder { get; set; }
}

public class WeekTemplate
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("site")]
    public string Site { get; set; }

    [JsonPropertyName("items")]
    public Schedule[] Items { get; set; }
}

public class Schedule
{
    [JsonPropertyName("days")]
    public DayOfWeek[] Days { get; set; }

    [JsonPropertyName("scheduleBlocks")]
    public ScheduleBlock[] ScheduleBlocks { get; set; }
}

public class TemplateAssignment
{    
    public DateOnly From { get; set; }
    public DateOnly Until { get; set; }
    public string TemplateId { get; set; }  
}