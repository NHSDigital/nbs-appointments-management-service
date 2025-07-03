namespace Nhs.Appointments.Core;

public class SiteReport
{
    public SiteReport(Site site, DaySummary[] days, string[] clinicalServices)
    {
        SiteName = site.Name;
        ICB = site.IntegratedCareBoard;
        Region = site.Region;
        OdsCode = site.OdsCode;
        Longitude = site.Location.Coordinates[0];
        Latitude = site.Location.Coordinates[1];
        Bookings = clinicalServices.ToDictionary(
            service => service, 
            service => days.Sum(day => day.Sessions.Sum(x => x.Bookings.GetValueOrDefault(service, 0))));
        Cancelled = days.Sum(day => day.CancelledAppointments);
        Orphaned = days.Sum(day => day.OrphanedAppointments);
        RemainingCapacity = clinicalServices.ToDictionary(
            service => service, 
            service => days.Sum(day => day.Sessions.Sum(x => x.Bookings.ContainsKey(service) ? x.RemainingCapacity : 0)));
        MaximumCapacity = days.Sum(day => day.MaximumCapacity);
    }
    
    public string SiteName { get; }
    public string ICB { get; }
    public string Region { get; }
    public string OdsCode { get; }
    public double Longitude { get; }
    public double Latitude { get; }
    public Dictionary<string, int> Bookings { get; }
    public int TotalBookings => Bookings.Sum(x => x.Value);
    public int Cancelled { get; }
    public int Orphaned { get; }
    public Dictionary<string, int> RemainingCapacity { get; }
    public int MaximumCapacity { get; }
}
