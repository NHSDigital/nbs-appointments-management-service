namespace Nhs.Appointments.Core.Reports.SiteSummary;

public class SiteSummaryOptions
{
    public int DaysForward { get; set; }
    public int DaysChunkSize { get; set; }
    public DateOnly FirstRunDate { get; set; }
}
