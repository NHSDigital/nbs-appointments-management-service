namespace Nhs.Appointments.Core.Reports.SiteSummary;

public class AggregateSiteSummaryEvent
{
    public string Site { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly To { get; set; }
}
