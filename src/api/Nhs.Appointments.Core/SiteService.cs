using Microsoft.Extensions.Caching.Memory;

namespace Nhs.Appointments.Core;

public interface ISiteService
{
    Task<IEnumerable<SiteWithDistance>> FindSitesByArea(double longitude, double latitude, int searchRadius, int maximumRecords, IEnumerable<string> accessNeeds, bool ignoreCache);
    Task<Site> GetSiteByIdAsync(string siteId, string scope = "*");
    Task<IEnumerable<SitePreview>> GetSitesPreview();
    Task<OperationResult> UpdateSiteAttributesAsync(string siteId, string scope, IEnumerable<AttributeValue> attributeValues);    
}

public class SiteService(ISiteStore siteStore, IMemoryCache memoryCache, TimeProvider time) : ISiteService
{
    private const string CacheKey = "sites";
    public async Task<IEnumerable<SiteWithDistance>> FindSitesByArea(double longitude, double latitude, int searchRadius, int maximumRecords, IEnumerable<string> accessNeeds, bool ignoreCache = false)
    {        
        var attributeIds = accessNeeds.Where(an => string.IsNullOrEmpty(an) == false).Select(an => $"accessibility/{an}").ToList();

        var sites = memoryCache.Get(CacheKey) as IEnumerable<Site>;
        if (sites == null || ignoreCache)
        {
            sites = await siteStore.GetAllSites();            
            memoryCache.Set(CacheKey, sites, time.GetUtcNow().AddMinutes(10));
        }

        var sitesWithDistance = sites
                .Select(s => new SiteWithDistance(s, CalculateDistanceInMetres(s.Location.Coordinates[1], s.Location.Coordinates[0], latitude, longitude)));

        Func<SiteWithDistance, bool> filterPredicate = attributeIds.Any() ?
            s => attributeIds.All(attr => s.Site.AttributeValues.SingleOrDefault(a => a.Id == attr)?.Value == "true") :
            s => true;
        
        return sitesWithDistance
            .Where(s => s.Distance <= searchRadius)
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

    public async Task<IEnumerable<SitePreview>> GetSitesPreview()
    {
        var sites = memoryCache.Get(CacheKey) as IEnumerable<Site>;
        if (sites == null)
        {
            sites = await siteStore.GetAllSites();
            memoryCache.Set(CacheKey, sites, time.GetUtcNow().AddMinutes(10));
        }

        return sites.Select(s => new SitePreview(s.Id, s.Name));
    }

    public Task<OperationResult> UpdateSiteAttributesAsync(string siteId, string scope, IEnumerable<AttributeValue> attributeValues)
    {
        return siteStore.UpdateSiteAttributes(siteId, scope, attributeValues);
    }

    private int CalculateDistanceInMetres(double lat1, double lon1, double lat2, double lon2)
    {
        var epsilon = 0.000001f;
        var deltaLatitude = lat1 - lat2;
        var deltaLongitude = lon1 - lon2;

        if( Math.Abs(deltaLatitude) < epsilon && Math.Abs(deltaLongitude) < epsilon )
            return 0;

        var dist = Math.Sin(DegreesToRadians(lat1)) * Math.Sin(DegreesToRadians(lat2)) + Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) * Math.Cos(DegreesToRadians(deltaLongitude));
        dist = Math.Acos(dist);
        dist = RadiansToDegrees(dist);
        return (int)(dist * 60 * 1.1515 * 1.609344 * 1000);        
    }

    private double DegreesToRadians(double deg) => (deg * Math.PI / 180.0);

    private double RadiansToDegrees(double rad) => (rad / Math.PI * 180.0);    
}
