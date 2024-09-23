namespace Nhs.Appointments.Core;

public interface ISiteService
{
    Task<IEnumerable<SiteWithDistance>> FindSitesByArea(double longitude, double latitude, int searchRadius, int maximumRecords);
    Task<Site> GetSiteByIdAsync(string siteId);
    Task<OperationResult> UpdateSiteAttributesAsync(string siteId, IEnumerable<AttributeValue> attributeValues);
}

public class SiteService(ISiteStore siteStore) : ISiteService
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
    
    public Task<OperationResult> UpdateSiteAttributesAsync(string siteId, IEnumerable<AttributeValue> attributeValues)
    {
        return siteStore.UpdateSiteAttributes(siteId, attributeValues);
    }
}
