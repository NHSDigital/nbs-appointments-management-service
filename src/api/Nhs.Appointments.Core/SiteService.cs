namespace Nhs.Appointments.Core;

public interface ISiteService
{
    Task<IEnumerable<SiteWithDistance>> FindSitesByArea(double longitude, double latitude, int searchRadius, int maximumRecords, IEnumerable<string> accessNeeds);
    Task<Site> GetSiteByIdAsync(string siteId);
    Task<OperationResult> UpdateSiteAttributesAsync(string siteId, IEnumerable<AttributeValue> attributeValues);
}

public class SiteService(ISiteStore siteStore) : ISiteService
{
    public async Task<IEnumerable<SiteWithDistance>> FindSitesByArea(double longitude, double latitude, int searchRadius, int maximumRecords, IEnumerable<string> accessNeeds)
    {
        var attributeIds = accessNeeds.Where(an => string.IsNullOrEmpty(an) == false).Select(an => $"accessibility/{an}").ToList();
        var sites = await siteStore.GetSitesByArea(longitude, latitude, searchRadius);
        Func<SiteWithDistance, bool> filterPredicate = attributeIds.Any() ?
            s => attributeIds.All(attr => s.Site.AttributeValues.SingleOrDefault(a => a.Id == attr)?.Value == "true") :
            s => true;
        
        return sites
            .Where(filterPredicate) 
            .OrderBy(site => site.Distance)
            .Take(maximumRecords);
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
