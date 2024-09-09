namespace Nhs.Appointments.Core;

public interface ISiteSearchService
{
    Task<IEnumerable<SiteWithDistance>> FindSitesByArea(double longitude, double latitude, int searchRadius, int maximumRecords);
    Task<Site> GetSiteByIdAsync(string siteId);
}

public class SiteSearchService(ISiteStore siteStore) : ISiteSearchService
{
    public async Task<IEnumerable<SiteWithDistance>> FindSitesByArea(double longitude, double latitude, int searchRadius, int maximumRecords)
    {
        var unorderedSites = await siteStore.GetSitesByArea(longitude, latitude, searchRadius);
        var orderedSites = unorderedSites.OrderBy(site => site.Distance).Take(maximumRecords);
        return orderedSites;
    }
    
    public Task<Site> GetSiteByIdAsync(string siteId)
    {
        return siteStore.GetSiteById(siteId);
    }
}
