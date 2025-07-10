namespace Nhs.Appointments.Core.Reports.SiteSummary;

public class DailySiteSummary
{
    public string Site { get; set; }
    public DateOnly Date { get; set; }
    public Dictionary<string, int> Bookings { get; set; }
    public int Cancelled { get; set; }
    public Dictionary<string, int> Orphaned { set; get; }
    public Dictionary<string, int> RemainingCapacity { set; get; }
    public int MaximumCapacity { get; }
    public DateTimeOffset GeneratedAtUtc { get; set; }
}
