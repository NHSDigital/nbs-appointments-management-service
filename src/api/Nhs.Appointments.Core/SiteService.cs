using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Core;

public interface ISiteService
{
    Task<IEnumerable<SiteWithDistance>> FindSitesByArea(double longitude, double latitude, int searchRadius,
        int maximumRecords, IEnumerable<string> accessNeeds, bool ignoreCache, SiteSupportsServiceFilter siteSupportsServiceFilter = null);

    Task<Site> GetSiteByIdAsync(string siteId, string scope = "*");
    Task<IEnumerable<SitePreview>> GetSitesPreview(bool includeDeleted = false);
    Task<IEnumerable<Site>> GetAllSites(bool includeDeleted = false, bool ignoreCache = false);
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
    Task<OperationResult> ToggleSiteSoftDeletionAsync(string siteId);
}

public class SiteService(
    ISiteStore siteStore,
    IAvailabilityStore availabilityStore,
    IMemoryCache memoryCache,
    ILogger<ISiteService> logger,
    TimeProvider time,
    IFeatureToggleHelper featureToggleHelper,
    IOptions<SiteServiceOptions> options) : ISiteService
{
    public async Task<IEnumerable<SiteWithDistance>> FindSitesByArea(double longitude, double latitude, int searchRadius, int maximumRecords, IEnumerable<string> accessNeeds, bool ignoreCache = false, SiteSupportsServiceFilter siteSupportsServiceFilter = null)
    {        
        var accessibilityIds = accessNeeds.Where(an => string.IsNullOrEmpty(an) == false).Select(an => $"accessibility/{an}").ToList();

        var sites = await GetAllSites(false, ignoreCache);

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
        if (site is null || site.isDeleted is true)
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

    public async Task<IEnumerable<Site>> GetAllSites(bool includeDeleted = false, bool ignoreCache = false)
    {
        // ignoreCache is an optional param an external caller can provide through only certain API routes,
        // whereas DisableSiteCache is a global setting affecting all uses
        if (ignoreCache || options.Value.DisableSiteCache)
        {
            return await siteStore.GetAllSites(includeDeleted);
        }

        var sites = memoryCache.Get(options.Value.SiteCacheKey) as IEnumerable<Site>;
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

    public async Task<OperationResult> ToggleSiteSoftDeletionAsync(string siteId)
    {
        var result = await siteStore.ToggleSiteSoftDeletionAsync(siteId);
        if (!result.Success)
        {
            return result;
        }

        if (!options.Value.DisableSiteCache && memoryCache.TryGetValue(options.Value.SiteCacheKey, out _))
        {
            memoryCache.Remove(options.Value.SiteCacheKey);
        }

        return result;
    }

    public async Task<IEnumerable<SiteWithDistance>> QuerySitesAsync(SiteFilter[] filters, int maxRecords, bool ignoreCache)
    {
        var sites = await GetAllSites(false, ignoreCache);

        if (await featureToggleHelper.IsFeatureEnabled(Flags.SiteStatus))
        {
            sites = sites.Where(s => s.status is SiteStatus.Online or null);
        }

        var allResults = new List<SiteWithDistance>();

        var orderedFilters = OrderFiltersByPriority(filters);

        foreach (var filter in orderedFilters)
        {
            // No need to keep processing if we've hit the maxRecords count
            if (allResults.Count >= maxRecords)
            {
                break;
            }

            var predicate = BuildPredicate(filter);
            var filteredSites = sites.Where(predicate);

            var sitesWithDistance = filteredSites
                .Select(s => new SiteWithDistance(
                    s,
                    CalculateDistanceInMetres(
                        s.Location.Coordinates[1],
                        s.Location.Coordinates[0],
                        filter.Latitude,
                        filter.Longitude)))
                .ToList();

            if (filter.Availability is not null && filter.Availability.Services?.Length > 0)
            {
                // Adding .Single() here as the current implementation only allows for filtering on a single service
                // This will need updating if we decide to allow filtering on multiple services
                var siteSupportsServiceFilter = new SiteSupportsServiceFilter(filter.Availability.Services.Single(), filter.Availability.From!.Value, filter.Availability.Until!.Value);

                var serviceResults = await GetSitesSupportingService(
                    sitesWithDistance,
                    siteSupportsServiceFilter.service,
                    siteSupportsServiceFilter.from,
                    siteSupportsServiceFilter.until);

                sitesWithDistance = [.. serviceResults.DistinctBy(swd => swd.Site.Id)];
            }

            allResults.AddRange(sitesWithDistance);
        }

        return allResults
            .DistinctBy(swd => swd.Site.Id)
            .OrderBy(swd => swd.Distance)
            .Take(maxRecords)
            .ToList();
    }

    private Func<Site, bool> BuildPredicate(SiteFilter filter)
    {
        var hasAccessNeedsFilter = filter.AccessNeeds is not null && filter.AccessNeeds.Length > 0;
        var hasSiteTypeFilter = filter.Types is not null && filter.Types.Length > 0;

        var accessibilityIds = new List<string>();
        if (hasAccessNeedsFilter)
        {
            accessibilityIds = [.. filter.AccessNeeds
                .Where(an => !string.IsNullOrEmpty(an))
                .Select(an => $"accessibility/{an}")];
        }

        return site =>
        {
            if (hasAccessNeedsFilter)
            {
                if (!accessibilityIds.All(acc => site.Accessibilities.SingleOrDefault(a => a.Id == acc)?.Value == "true"))
                {
                    return false;
                }
            }

            if (hasSiteTypeFilter)
            {
                var (includedTypes, excludedTypes) = ParseSiteTypeFilters(filter.Types);

                if (includedTypes.Count > 0 && !includedTypes.Any(st => st.Equals(site.Type, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return false;
                }

                if (excludedTypes.Count > 0 && excludedTypes.Any(st => st.Equals(site.Type, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(filter.OdsCode))
            {
                return filter.OdsCode.Equals(site.OdsCode, StringComparison.InvariantCultureIgnoreCase);
            }

            var distance = CalculateDistanceInMetres(site.Location.Coordinates[1], site.Location.Coordinates[0], filter.Latitude, filter.Longitude);
            return distance <= filter.SearchRadius;
        };
    }

    private static (List<string> IncludedSiteTypes, List<string> ExcludedSiteTypes) ParseSiteTypeFilters(string[] siteTypes)
    {
        var includedTypes = new List<string>();
        var excludedTypes = new List<string>();

        const char excludesChar = '!';

        foreach (var type in siteTypes)
        {
            if (type.StartsWith(excludesChar))
            {
                excludedTypes.Add(type[1..].Trim());
            }
            else
            {
                includedTypes.Add(type);
            }
        }

        return (includedTypes, excludedTypes);
    }

    private static IEnumerable<SiteFilter> OrderFiltersByPriority(SiteFilter[] filters)
    {
        var indexed = filters.Select((f, i) => new { Filter = f, Index = i }).ToList();
        var anyFilterHasPriority = indexed.Any(x => x.Filter.Priority.HasValue);

        return anyFilterHasPriority
            ? indexed
                .OrderBy(x => x.Filter.Priority.HasValue ? 0 : 1) // Any filter with the priority prop set moves to the front
                .ThenBy(x => x.Filter.Priority ?? int.MaxValue) // Then order by prioirty ascending
                .ThenBy(x => x.Index) // Then order by index ascending if any filters have no priority set
                .Select(x => x.Filter)
                .ToList()
            : filters;
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
        memoryCache.Set(options.Value.SiteCacheKey, sites,
            time.GetUtcNow().AddMinutes(options.Value.SiteCacheDuration));

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
}
