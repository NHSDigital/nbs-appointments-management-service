using Nhs.Appointments.Core.Reports.SiteSummary;

namespace Nhs.Appointments.Core;

public class SiteReport
{
    public SiteReport(Site site, DailySiteSummary[] days, string[] clinicalServices)
    {
        var cancellationReasons = days.SelectMany(x => x.Cancelled.Keys).Distinct();
        
        SiteName = site.Name;
        ICB = site.IntegratedCareBoard;
        Region = site.Region;
        OdsCode = site.OdsCode;
        Longitude = site.Location.Coordinates[0];
        Latitude = site.Location.Coordinates[1];
        Bookings = clinicalServices.ToDictionary(
            service => service, 
            service => days.Sum(day => day.Bookings.GetValueOrDefault(service, 0)) + days.Sum(x => x.Orphaned.GetValueOrDefault(service, 0)));
        Cancelled = cancellationReasons.ToDictionary(
            reason => reason,
            reason => days.Sum(day => day.Cancelled.GetValueOrDefault(reason, 0)));
        RemainingCapacity = clinicalServices.ToDictionary(
            service => service, 
            service => days.Sum(day => day.RemainingCapacity.GetValueOrDefault(service, 0)));
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
    public Dictionary<string, int> Cancelled { get; }
    public Dictionary<string, int> RemainingCapacity { get; }
    public int MaximumCapacity { get; }
}
