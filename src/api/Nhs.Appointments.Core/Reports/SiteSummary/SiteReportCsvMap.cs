namespace Nhs.Appointments.Core.Reports.SiteSummary;

public static class SiteReportMap
{
    public static string[] Headers(string[] services)
    {
        var siteHeaders = new[]
        {
            "Site Name", "Site Type", "ICB", "ICB Name", "Region", "Region Name", "ODS Code", "Longitude", "Latitude"
        };
        var statHeaders = new[] { "Total Bookings", "Cancelled", "Maximum Capacity" };
        var bookingsHeaders = services.Select(service => $"{service} Booked");
        var capacityHeaders = services.Select(service => $"{service} Capacity");

        return siteHeaders
            .Union(bookingsHeaders
                .Union(statHeaders))
            .Union(capacityHeaders)
            .ToArray();
    }

    public static string SiteName(SiteReport report) => report.SiteName;
    public static string SiteType(SiteReport report) => report.SiteType;
    public static string ICB(SiteReport report) => report.ICB;
    public static string ICBName(SiteReport report) => report.ICBName;
    public static string Region(SiteReport report) => report.Region;
    public static string RegionName(SiteReport report) => report.RegionName;
    public static string OdsCode(SiteReport report) => report.OdsCode;
    public static double Longitude(SiteReport report) => report.Longitude;
    public static double Latitude(SiteReport report) => report.Latitude;
    public static int TotalBookings(SiteReport report) => report.TotalBookings;
    public static int Cancelled(SiteReport report) => report.Cancelled;
    public static int MaximumCapacity(SiteReport report) => report.MaximumCapacity;

    public static int BookingsCount(SiteReport report, string key) =>
        report.Bookings.TryGetValue(key, out var booking) ? booking : 0;

    public static int CapacityCount(SiteReport report, string key) =>
        report.RemainingCapacity.TryGetValue(key, out var capacity) ? capacity : 0;
}
