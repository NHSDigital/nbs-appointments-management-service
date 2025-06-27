namespace Nhs.Appointments.Core;

public class SiteReport
{
    public SiteReport(Site site, DaySummary[] days, string[] clinicalServices)
    {
        SiteName = site.Name;
        ICB = site.IntegratedCareBoard;
        Region = site.Region;
        OdsCode = site.OdsCode;
        Bookings = clinicalServices.ToDictionary(
            service => service, 
            service => days.Sum(day => day.Sessions.Sum(x => x.Bookings.TryGetValue(service, out var serviceBookings) ? serviceBookings : 0)));
        Cancelled = days.Sum(day => day.CancelledAppointments);
        Orphaned = days.Sum(day => day.OrphanedAppointments);
        RemainingCapacity = clinicalServices.ToDictionary(
            service => service, 
            service => days.Sum(day => day.Sessions.Sum(x => x.Bookings.ContainsKey(service) ? x.RemainingCapacity : 0)));
    }
    
    public string SiteName { get; set; }
    public string ICB { get; set; }
    public string Region { get; set; }
    public string OdsCode { get; set; }
    public Dictionary<string, int> Bookings { get; set; }
    public int TotalBookings => Bookings.Sum(x => x.Value);
    public int Cancelled { get; set; }
    public int Orphaned { get; set; }
    public Dictionary<string, int> RemainingCapacity { get; set; }
}
