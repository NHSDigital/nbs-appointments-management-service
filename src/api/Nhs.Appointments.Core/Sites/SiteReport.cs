using Nhs.Appointments.Core.OdsCodes;
using Nhs.Appointments.Core.Reports.SiteSummary;

namespace Nhs.Appointments.Core.Sites;

public class SiteReport
{
    public SiteReport(Site site, DailySiteSummary[] days, string[] clinicalServices,
        IEnumerable<WellKnownOdsEntry> wellKnownOdsCodes)
    {
        var regionName = wellKnownOdsCodes.SingleOrDefault(code => code.Type == "region" && code.OdsCode == site.Region)
            ?.DisplayName ?? "blank";
        var icbName =
            wellKnownOdsCodes.SingleOrDefault(code => code.Type == "icb" && code.OdsCode == site.IntegratedCareBoard)
                ?.DisplayName ?? "blank";

        SiteName = site.Name;
        Status = (site.status ?? SiteStatus.Online).ToString();
        SiteType = site.Type;
        ICB = site.IntegratedCareBoard;
        Region = site.Region;
        OdsCode = site.OdsCode;
        Longitude = site.Coordinates.Longitude;
        Latitude = site.Coordinates.Latitude;
        RegionName = regionName;
        ICBName = icbName;
        Bookings = clinicalServices.ToDictionary(
            service => service, 
            service => days.Sum(day => day.Bookings.GetValueOrDefault(service, 0)) + days.Sum(x => x.Orphaned.GetValueOrDefault(service, 0)));
        Cancelled = days.Sum(day => day.Cancelled);
        RemainingCapacity = clinicalServices.ToDictionary(
            service => service, 
            service => days.Sum(day => day.RemainingCapacity.GetValueOrDefault(service, 0)));
        MaximumCapacity = days.Sum(day => day.MaximumCapacity);
    }
    
    public string SiteName { get; }
    public string Status { get; }
    public string SiteType { get; }
    public string ICB { get; }
    public string ICBName { get; }
    public string Region { get; }
    public string RegionName { get; }
    public string OdsCode { get; }
    public double Longitude { get; }
    public double Latitude { get; }
    public Dictionary<string, int> Bookings { get; }
    public int TotalBookings => Bookings.Sum(x => x.Value);
    public int Cancelled { get; }
    public Dictionary<string, int> RemainingCapacity { get; }
    public int MaximumCapacity { get; }
}
