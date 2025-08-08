using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Nhs.Appointments.Core;

public interface ISiteService
{
    Task<IEnumerable<SiteWithDistance>> FindSitesByArea(double longitude, double latitude, int searchRadius,
        int maximumRecords, IEnumerable<string> accessNeeds, bool ignoreCache, SiteSupportsServiceFilter siteSupportsServiceFilter = null);

    Task<Site> GetSiteByIdAsync(string siteId, string scope = "*");
    Task<IEnumerable<SitePreview>> GetSitesPreview();
    Task<OperationResult> UpdateAccessibilities(string siteId, IEnumerable<Accessibility> accessibilities);
    Task<OperationResult> UpdateInformationForCitizens(string siteId, string informationForCitizens);

    Task<OperationResult> UpdateSiteDetailsAsync(string siteId, string name, string address, string phoneNumber,
        decimal? longitude, decimal? latitude);

    Task<OperationResult> UpdateSiteReferenceDetailsAsync(string siteId, string odsCode, string icb, string region);

    Task<OperationResult> SaveSiteAsync(string siteId, string odsCode, string name, string address, string phoneNumber,
        string icb, string region, Location location, IEnumerable<Accessibility> accessibilities, string type);
    Task<IEnumerable<Site>> GetSitesInRegion(string region);
}

public class SiteService(ISiteStore siteStore, IAvailabilityStore availabilityStore, IMemoryCache memoryCache, ILogger<ISiteService> logger, TimeProvider time) : ISiteService
{
    private const string CacheKey = "sites";
    public async Task<IEnumerable<SiteWithDistance>> FindSitesByArea(double longitude, double latitude, int searchRadius, int maximumRecords, IEnumerable<string> accessNeeds, bool ignoreCache = false, SiteSupportsServiceFilter siteSupportsServiceFilter = null)
    {        
        var accessibilityIds = accessNeeds.Where(an => string.IsNullOrEmpty(an) == false).Select(an => $"accessibility/{an}").ToList();

        var sites = memoryCache.Get(CacheKey) as IEnumerable<Site>;
        if (sites == null || ignoreCache)
        {
            sites = await GetAndCacheSites();
        }

        var sitesWithDistance = sites
            .Select(s => new SiteWithDistance(s,
                CalculateDistanceInMetres(s.Location.Coordinates[1], s.Location.Coordinates[0], latitude, longitude)));

        Func<SiteWithDistance, bool> filterPredicate = accessibilityIds.Any() ?
            s => accessibilityIds.All(acc => s.Site.Accessibilities.SingleOrDefault(a => a.Id == acc)?.Value == "true") :
            s => true;

        if (siteSupportsServiceFilter == null)
        {
            return sitesWithDistance
                .Where(s => s.Distance <= searchRadius)
                .Where(filterPredicate)
                .OrderBy(site => site.Distance)
                .Take(maximumRecords);
        }
        
        var sitesInDistance = sitesWithDistance
            .Where(s => s.Distance <= searchRadius)
            .Where(filterPredicate);
        
        return await GetSitesSupportingService(
            sitesInDistance, 
            siteSupportsServiceFilter.service, 
            siteSupportsServiceFilter.from,
            siteSupportsServiceFilter.until,
            maximumRecords,
            maximumRecords * 2);
    }

    private async Task<IEnumerable<SiteWithDistance>> GetSitesSupportingService(IEnumerable<SiteWithDistance> sites, string service, DateOnly from, DateOnly to,
        int maxRecords = 50, int batchSize = 100)
    {
        var orderedSites = sites.OrderBy(site => site.Distance).ToList();
        
        var results = new List<SiteWithDistance>();

        var dateStringsInRange = GetDateStringsInRange(from, to);
        var iterations = 0;
        
        //while we are still short of the max, keep appending results
        //ideally, the first batch would contain more than or equal to the max results, so won't need to iterate often...
        while (results.Count < maxRecords)
        {
            var concurrentBatchResults = new ConcurrentBag<SiteWithDistance>();
            
            var orderedSiteBatch = orderedSites.Skip(iterations * batchSize).Take(batchSize).ToList();

            //break out if no more sites to query, just have to return the built results, this is likely to be less than the maxResults
            if (orderedSiteBatch.Count == 0)
            {
                break;
            }

            var siteOffersServiceDuringPeriodTasks = orderedSiteBatch.Select(async swd =>
            {
                var siteOffersServiceDuringPeriod = await availabilityStore.SiteOffersServiceDuringPeriod(swd.Site.Id, service, dateStringsInRange);
                if (siteOffersServiceDuringPeriod)
                {
                    concurrentBatchResults.Add(swd);
                }
            }).ToArray();

            await Task.WhenAll(siteOffersServiceDuringPeriodTasks);
            
            //the concurrentBatchResults lose their original order, so we need to order the end result
            results.AddRange(concurrentBatchResults.OrderBy(site => site.Distance).Take(maxRecords));
            iterations++;
        }
        
        logger.LogInformation("GetSitesSupportingService returned {resultCount} result(s) after {iterationCount} iteration(s) for service '{service}'", results.Count, iterations, service);

        return results;
    }
    
       private static List<string> GetDateStringsInRange(DateOnly from, DateOnly to)
    {
        var result = new List<string>();

        if (to < from)
        {
            throw new ArgumentException("'to' date must be on or after 'from' date.");
        }

        for (var date = from; date <= to; date = date.AddDays(1))
        {
            result.Add(date.ToString("yyyyMMdd"));
        }

        return result;
    }
    
    public async Task<Site> GetSiteByIdAsync(string siteId, string scope = "*")
    {
        var site = await siteStore.GetSiteById(siteId);
        if (site is null)
        {
            return default;
        }

        if (scope == "*")
        {
            return site;
        }

        site.Accessibilities =
            site.Accessibilities.Where(a => a.Id.Contains($"{scope}/", StringComparison.CurrentCultureIgnoreCase));

        return site;
    }

    public async Task<IEnumerable<Site>> GetSitesInRegion(string region)
        => await siteStore.GetSitesInRegionAsync(region);

    public async Task<IEnumerable<SitePreview>> GetSitesPreview()
    {
        var sites = memoryCache.Get(CacheKey) as IEnumerable<Site>;
        sites ??= await GetAndCacheSites();

        return sites.Select(s => new SitePreview(s.Id, s.Name, s.OdsCode));
    }

    public async Task<OperationResult> SaveSiteAsync(string siteId, string odsCode, string name, string address, string phoneNumber, string icb,
        string region, Location location, IEnumerable<Accessibility> accessibilities, string type)
            => await siteStore.SaveSiteAsync(
                siteId,
                odsCode,
                name,
                address,
                phoneNumber,
                icb,
                region,
                location,
                accessibilities,
                type);

    public Task<OperationResult> UpdateAccessibilities(string siteId, IEnumerable<Accessibility> accessibilities) 
    {
        return siteStore.UpdateAccessibilities(siteId, accessibilities);
    }

    public Task<OperationResult> UpdateInformationForCitizens(string siteId, string informationForCitizens) 
    {
        return siteStore.UpdateInformationForCitizens(siteId, informationForCitizens);
    }

    public Task<OperationResult> UpdateSiteDetailsAsync(string siteId, string name, string address, string phoneNumber,
        decimal? longitude, decimal? latitude)
    {
        return siteStore.UpdateSiteDetails(siteId, name, address, phoneNumber, longitude, latitude);
    }

    public Task<OperationResult> UpdateSiteReferenceDetailsAsync(string siteId, string odsCode, string icb,
        string region)
    {
        return siteStore.UpdateSiteReferenceDetails(siteId, odsCode, icb, region);
    }

    private int CalculateDistanceInMetres(double lat1, double lon1, double lat2, double lon2)
    {
        var epsilon = 0.000001f;
        var deltaLatitude = lat1 - lat2;
        var deltaLongitude = lon1 - lon2;

        if (Math.Abs(deltaLatitude) < epsilon && Math.Abs(deltaLongitude) < epsilon)
        {
            return 0;
        }

        var dist = Math.Sin(DegreesToRadians(lat1)) * Math.Sin(DegreesToRadians(lat2)) +
                   Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                   Math.Cos(DegreesToRadians(deltaLongitude));
        dist = Math.Acos(dist);
        dist = RadiansToDegrees(dist);
        return (int)(dist * 60 * 1.1515 * 1.609344 * 1000);
    }

    private double DegreesToRadians(double deg) => deg * Math.PI / 180.0;

    private double RadiansToDegrees(double rad) => rad / Math.PI * 180.0;

    private async Task<IEnumerable<Site>> GetAndCacheSites()
    {
        var sites = await siteStore.GetAllSites();
        memoryCache.Set(CacheKey, sites, time.GetUtcNow().AddMinutes(10));

        return sites;
    }
}
