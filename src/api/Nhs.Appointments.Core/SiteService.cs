using Microsoft.Extensions.Caching.Memory;

namespace Nhs.Appointments.Core;

public interface ISiteService
{
    Task<IEnumerable<SiteWithDistance>> FindSitesByArea(double longitude, double latitude, int searchRadius, int maximumRecords, IEnumerable<string> accessNeeds, bool ignoreCache);
    Task<Site> GetSiteByIdAsync(string siteId, string scope = "*");
    Task<OperationResult> UpdateSiteAttributesAsync(string siteId, string scope, IEnumerable<AttributeValue> attributeValues);    
}

public class SiteService(ISiteStore siteStore, IMemoryCache memoryCache, TimeProvider time) : ISiteService
{
    public async Task<IEnumerable<SiteWithDistance>> FindSitesByArea(double longitude, double latitude, int searchRadius, int maximumRecords, IEnumerable<string> accessNeeds, bool ignoreCache = false)
    {        
        var attributeIds = accessNeeds.Where(an => string.IsNullOrEmpty(an) == false).Select(an => $"accessibility/{an}").ToList();

        var sites = memoryCache.Get("sites") as IEnumerable<Site>;
        if (sites == null || ignoreCache)
        {
            sites = await siteStore.GetAllSites();            
            memoryCache.Set("sites", sites, time.GetUtcNow().AddMinutes(10));
        }

        var sitesWithDistance = sites
                .Select(s => new SiteWithDistance(s, (int)(CalculateDistance(s.Location.Coordinates[1], s.Location.Coordinates[0], latitude, longitude, 'K') * 1000)));

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

    public Task<OperationResult> UpdateSiteAttributesAsync(string siteId, string scope, IEnumerable<AttributeValue> attributeValues)
    {
        return siteStore.UpdateSiteAttributes(siteId, scope, attributeValues);
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2, char unit)
    {
        if ((lat1 == lat2) && (lon1 == lon2))
        {
            return 0;
        }
        else
        {
            double theta = lon1 - lon2;
            double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
            dist = Math.Acos(dist);
            dist = rad2deg(dist);
            dist = dist * 60 * 1.1515;
            if (unit == 'K')
            {
                dist = dist * 1.609344;
            }
            else if (unit == 'N')
            {
                dist = dist * 0.8684;
            }
            return (dist);
        }
    }

    //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    //::  This function converts decimal degrees to radians             :::
    //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    private double deg2rad(double deg)
    {
        return (deg * Math.PI / 180.0);
    }

    //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    //::  This function converts radians to decimal degrees             :::
    //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
    private double rad2deg(double rad)
    {
        return (rad / Math.PI * 180.0);
    }
}
