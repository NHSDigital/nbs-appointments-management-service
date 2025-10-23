using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Core;

public interface ISiteService
{
    Task<IEnumerable<SiteWithDistance>> FindSitesByArea(double longitude, double latitude, int searchRadius,
        int maximumRecords, IEnumerable<string> accessNeeds, bool ignoreCache, SiteSupportsServiceFilter siteSupportsServiceFilter = null);

    Task<Site> GetSiteByIdAsync(string siteId, string scope = "*");
    Task<IEnumerable<SitePreview>> GetSitesPreview(bool includeDeleted = false);
    Task<IEnumerable<Site>> GetAllSites(bool includeDeleted = false);
    Task<OperationResult> UpdateAccessibilities(string siteId, IEnumerable<Accessibility> accessibilities);
    Task<OperationResult> UpdateInformationForCitizens(string siteId, string informationForCitizens);

    Task<OperationResult> UpdateSiteDetailsAsync(string siteId, string name, string address, string phoneNumber,
        decimal? longitude, decimal? latitude);

    Task<OperationResult> UpdateSiteReferenceDetailsAsync(string siteId, string odsCode, string icb, string region);

    Task<OperationResult> SaveSiteAsync(string siteId, string odsCode, string name, string address, string phoneNumber,
        string icb, string region, Location location, IEnumerable<Accessibility> accessibilities, string type, SiteStatus? siteStatus = null);
    Task<IEnumerable<Site>> GetSitesInRegion(string region);
    Task<OperationResult> SetSiteStatus(string siteId, SiteStatus status);
    Task<IEnumerable<Site>> GetSitesInIcbAsync(string icb);
    Task<IEnumerable<SiteWithDistance>> QuerySitesAsync(SiteFilter[] filters, int maxRecords, bool ignoreCache);
}

public class SiteService(ISiteStore siteStore, IAvailabilityStore availabilityStore, IMemoryCache memoryCache, ILogger<ISiteService> logger, TimeProvider time, IFeatureToggleHelper featureToggleHelper) : ISiteService
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

        if (await featureToggleHelper.IsFeatureEnabled(Flags.SiteStatus))
        {
            sites = sites.Where(s => s.status is SiteStatus.Online or null);
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
            maximumRecords * 20);
    }

    private async Task<IEnumerable<SiteWithDistance>> GetSitesSupportingService(IEnumerable<SiteWithDistance> sites, string service, DateOnly from, DateOnly to,
        int maxRecords = 50, int batchSize = 1000)
    {
        var orderedSites = sites.OrderBy(site => site.Distance).ToList();
        
        var results = new List<SiteWithDistance>();

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
                var siteOffersServiceDuringPeriod = await GetSiteSupportingServiceInRange(swd.Site.Id, service, from, to);
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
    
    public async Task<IEnumerable<Site>> GetAllSites(bool includeDeleted = false)
    {
        var sites = memoryCache.Get(CacheKey) as IEnumerable<Site>;
        sites ??= await GetAndCacheSites(includeDeleted);

        return sites;
    }
    
    public async Task<IEnumerable<SitePreview>> GetSitesPreview(bool includeDeleted = false)
    {
        var sites = await GetAllSites(includeDeleted);

        return sites.Select(s => new SitePreview(s.Id, s.Name, s.OdsCode, s.IntegratedCareBoard));
    }

    public async Task<OperationResult> SaveSiteAsync(string siteId, string odsCode, string name, string address, string phoneNumber, string icb,
        string region, Location location, IEnumerable<Accessibility> accessibilities, string type, SiteStatus? siteStatus = null)
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
                type,
                siteStatus);

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

    public async Task<OperationResult> SetSiteStatus(string siteId, SiteStatus status)
        => await siteStore.UpdateSiteStatusAsync(siteId, status);

    public async Task<IEnumerable<Site>> GetSitesInIcbAsync(string icb)
        => await siteStore.GetSitesInIcbAsync(icb);

    public async Task<IEnumerable<SiteWithDistance>> QuerySitesAsync(SiteFilter[] filters, int maxRecords, bool ignoreCache)
    {
        var sites = memoryCache.Get(CacheKey) as IEnumerable<Site>;
        if (sites == null || ignoreCache)
        {
            sites = await GetAndCacheSites();
        }

        if (await featureToggleHelper.IsFeatureEnabled(Flags.SiteStatus))
        {
            sites = sites.Where(s => s.status is SiteStatus.Online or null);
        }

        // Filters which don't involve any service availability calls
        var simpleFilters = filters
            .Where(f => f.Services is null || f.Services.Length is 0)
            .Select(BuildFilterPredicate)
            .ToList();

        var serviceFilters = filters.Where(f => f.Services is not null && f.Services.Length > 0);

        var sitesWithDistance = simpleFilters.Count == 0
            ? [.. sites.Select(s => new SiteWithDistance(s, int.MaxValue))] // No simple filters - distance will be determined on the service filter check
            : sites
                .Select(s => BuildSiteWithDistance(s, simpleFilters))
                .Where(swd => swd != null)
                .ToList();

        if (!serviceFilters.Any())
        {
            return sitesWithDistance
                .OrderBy(swd => swd.Distance)
                .Take(maxRecords)
                .ToList();
        }

        var serviceTasks = serviceFilters.Select(sf =>
        {
            // Adding .Single() here as the current implementation only allows for filtering on a single service
            // This will need updating if we decide to allow filtering on multiple services
            var filter = new SiteSupportsServiceFilter(sf.Services.Single(), sf.From!.Value, sf.Until!.Value);

            var filteredSites = sitesWithDistance
                .Where(s => CalculateDistanceInMetres(
                    s.Site.Location.Coordinates[1],
                    s.Site.Location.Coordinates[0],
                    sf.Latitude,
                    sf.Longitude) <= sf.SearchRadius);

            return GetSitesSupportingService(
                filteredSites,
                filter.service,
                filter.from,
                filter.until);
        });

        var serviceResults = (await Task.WhenAll(serviceTasks)).SelectMany(sr => sr);

        var combined = sitesWithDistance
            .Concat(serviceResults)
            .DistinctBy(swd => swd!.Site.Id)
            .OrderBy(swd => swd!.Distance)
            .Take(maxRecords)
            .ToList();

        return combined;
    }

    private FilterPredicate BuildFilterPredicate(SiteFilter filter)
    {
        var predicate = BuildPredicate(filter);
        return new FilterPredicate(predicate, filter.Latitude, filter.Longitude);
    }

    private Func<Site, bool> BuildPredicate(SiteFilter filter)
    {
        // TODO: APPT-1463 - add site type and ODS code filters

        var accessibilityIds = filter.AccessNeeds
            .Where(an => !string.IsNullOrEmpty(an))
            .Select(an => $"accessibility/{an}")
            .ToList();

        return site =>
        {
            if (filter.AccessNeeds.Length > 0)
            {
                if (!accessibilityIds.All(acc => site.Accessibilities.SingleOrDefault(a => a.Id == acc)?.Value == "true"))
                {
                    return false;
                }
            }

            var distance = CalculateDistanceInMetres(site.Location.Coordinates[1], site.Location.Coordinates[0], filter.Latitude, filter.Longitude);
            return distance <= filter.SearchRadius;
        };
    }

    private SiteWithDistance? BuildSiteWithDistance(Site site, List<FilterPredicate> filterPredicates)
    {
        var matchingDistances = filterPredicates
            .Where(fp => fp.Predicate(site))
            .Select(fp => CalculateDistanceInMetres(
                site.Location.Coordinates[1],
                site.Location.Coordinates[0],
                fp.Latitude,
                fp.Longitude))
            .ToList();

        return matchingDistances.Count == 0
            ? null
            : new SiteWithDistance(site, matchingDistances.Min());
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

    private async Task<IEnumerable<Site>> GetAndCacheSites(bool includeDeleted = false)
    {
        var sites = await siteStore.GetAllSites(includeDeleted);
        memoryCache.Set(CacheKey, sites, time.GetUtcNow().AddMinutes(10));

        return sites;
    }
    
    private async Task<bool> GetSiteSupportingServiceInRange(string siteId, string service, DateOnly from, DateOnly until)
    {
        var cacheKey = GetCacheSiteServiceSupportDateRangeKey(siteId, service, from, until);

        if (memoryCache.TryGetValue(cacheKey, out bool siteSupportsService))
        {
            return siteSupportsService;
        }
        
        var dateStringsInRange = GetDateStringsInRange(from, until);
        var siteOffersServiceDuringPeriod = await availabilityStore.SiteOffersServiceDuringPeriod(siteId, service, dateStringsInRange);
        
        memoryCache.Set(cacheKey, siteOffersServiceDuringPeriod, time.GetUtcNow().AddMinutes(15));
        return siteOffersServiceDuringPeriod;
    }

    private string GetCacheSiteServiceSupportDateRangeKey(string siteId, string service, DateOnly from, DateOnly until)
    {
        var dateRange = $"{from.ToString("yyyyMMdd")}_{until.ToString("yyyyMMdd")}";
        return $"site_{siteId}_supports_{service}_in_{dateRange}";
    }

    private record FilterPredicate(
        Func<Site, bool> Predicate,
        double Latitude,
        double Longitude
);
}
