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
        var attributeIds = accessNeeds.Select(an => $"accessibility/{an}");
        var sites = await siteStore.GetSitesByArea(longitude, latitude, searchRadius);
        
        return sites
            .Where(x => x.Site.AttributeValues
                .Where(sa => attributeIds
                    .Contains(sa.Id)).All(sa => sa.Value == "true"))
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
