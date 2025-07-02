using CapacityDataExtracts.Documents;
using Nhs.Appointments.Persistance.Models;

namespace CapacityDataExtracts;

public class CapacityDataConverter(IEnumerable<SiteDocument> sites)
{
    public string ExtractICB(SiteSessionInstance document) => sites.Single(s => s.Id == document.Site).IntegratedCareBoard;

    public string ExtractRegion(SiteSessionInstance document) => sites.Single(s => s.Id == document.Site).Region;

    public string ExtractSiteName(SiteSessionInstance document) => sites.Single(s => s.Id == document.Site).Name;

    public double ExtractLongitude(SiteSessionInstance document) => sites.Single(s => s.Id == document.Site).Location.Coordinates[0];

    public double ExtractLatitude(SiteSessionInstance document) => sites.Single(s => s.Id == document.Site).Location.Coordinates[1];

    public string ExtractOdsCode(SiteSessionInstance document) => sites.Single(s => s.Id == document.Site).OdsCode;

    public static string ExtractDate(SiteSessionInstance document) => document.From.ToString("yyyy-MM-dd");

    public static string ExtractTime(SiteSessionInstance document) => document.From.ToString("HH:mm:ss");

    public static string ExtractSlotLength(SiteSessionInstance document) => document.Duration.ToString("mm");

    public static int ExtractCapacity(SiteSessionInstance document) => document.Capacity;

    public static string[] ExtractService(SiteSessionInstance document) => document.Services;
}
