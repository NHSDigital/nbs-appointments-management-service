namespace Nhs.Appointments.Core;

public interface ISiteStore
{
    Task<IEnumerable<Site>> GetSitesByArea(double longitude, double latitude, int searchRadius);
}
