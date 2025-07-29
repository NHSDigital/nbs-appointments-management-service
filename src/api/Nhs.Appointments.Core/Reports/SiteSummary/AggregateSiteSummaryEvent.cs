namespace Nhs.Appointments.Core.Reports.SiteSummary;

public class AggregateSiteSummaryEvent
{
    public string Site { get; init; }
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
}
