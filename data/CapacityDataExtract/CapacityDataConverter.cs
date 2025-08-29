using CapacityDataExtracts.Documents;

namespace CapacityDataExtracts;

public static class CapacityDataConverter
{
    public static string ExtractICB(SiteSessionInstance document) => document.Site.IntegratedCareBoard;

    public static string ExtractRegion(SiteSessionInstance document) => document.Site.Region;

    public static string ExtractSiteName(SiteSessionInstance document) => document.Site.Name;

    public static double ExtractLongitude(SiteSessionInstance document) => document.Site.Location.Coordinates[0];

    public static double ExtractLatitude(SiteSessionInstance document) => document.Site.Location.Coordinates[1];

    public static string ExtractOdsCode(SiteSessionInstance document) => document.Site.OdsCode;

    public static string ExtractDate(SiteSessionInstance document) => document.From.ToString("yyyy-MM-dd");

    public static string ExtractTime(SiteSessionInstance document) => document.From.ToString("HH:mm:ss");

    public static string ExtractSlotLength(SiteSessionInstance document) => document.Duration.ToString("mm");

    public static int ExtractCapacity(SiteSessionInstance document) => document.Capacity;

    public static string[] ExtractService(SiteSessionInstance document) => document.Services;
}
