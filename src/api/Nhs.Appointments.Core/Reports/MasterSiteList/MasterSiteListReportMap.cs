using Nhs.Appointments.Core.Sites;

namespace Nhs.Appointments.Core.Reports.MasterSiteList;

public static class MasterSiteListReportMap
{
    public static string[] Headers()
    {
        var siteHeaders = new[]
        {
            "Site Name", "ODS Code", "Site Type", "Region", "ICB", "GUID", "IsDeleted", "Status", "Long", "Lat", "Address"
        };

        return siteHeaders;
    }

    public static string SiteName(Site site) => site.Name;
    public static string OdsCode(Site site) => site.OdsCode;
    public static string SiteType(Site site) => site.Type;
    public static string Region(Site site) => site.Region;
    public static string ICB(Site site) => site.IntegratedCareBoard;
    public static string Guid(Site site) => site.Id;
    public static bool IsDeleted(Site site) => site.isDeleted ?? false;
    public static string Status(Site site) => site.status.ToString();
    public static double Longitude(Site site) => site.Coordinates?.Longitude ?? 0;
    public static double Latitude(Site site) => site.Coordinates?.Latitude ?? 0;
    public static string Address(Site site) => site.Address;

}
