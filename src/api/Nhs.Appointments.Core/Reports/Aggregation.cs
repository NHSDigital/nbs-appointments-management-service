namespace Nhs.Appointments.Core.Reports;

public class Aggregation
{
    public DateTimeOffset LastTriggeredUtcDate { get; set; }
    
    public DateOnly FromDateOnly { get; set; }
    
    public DateOnly ToDateOnly { get; set; }
    
    public DateOnly LastRanToDateOnly { get; set; }
}
