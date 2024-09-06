namespace Nhs.Appointments.Core;

public interface ISiteStore
{
    Task<IEnumerable<SiteWithDistance>> GetSitesByArea(double longitude, double latitude, int searchRadius);
}
