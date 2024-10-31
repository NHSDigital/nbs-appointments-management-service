namespace Nhs.Appointments.Core;

public interface ISiteService
{
    Task<IEnumerable<SiteWithDistance>> FindSitesByArea(double longitude, double latitude, int searchRadius, int maximumRecords, IEnumerable<string> accessNeeds);
    Task<Site> GetSiteByIdAsync(string siteId, string scope = "*");
    Task<OperationResult> UpdateSiteAttributesAsync(string siteId, string scope, IEnumerable<AttributeValue> attributeValues);
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

    public async Task<Site> GetSiteByIdAsync(string siteId, string scope = "*")
    {
        var site = await siteStore.GetSiteById(siteId);
        if (site is null)
            return default;

        if (scope == "*")
            return site;

        site.AttributeValues = site.AttributeValues.Where(a => a.Id.Contains($"{scope}/", StringComparison.CurrentCultureIgnoreCase));

        return site;
    }

    public Task<OperationResult> UpdateSiteAttributesAsync(string siteId, string scope, IEnumerable<AttributeValue> attributeValues)
    {
        return siteStore.UpdateSiteAttributes(siteId, scope, attributeValues);
    }
}
