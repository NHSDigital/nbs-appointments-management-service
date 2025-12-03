using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Geography;

namespace Nhs.Appointments.Core.Sites;

public interface ISiteService
{
    Task<IEnumerable<SiteWithDistance>> FindSitesByArea(Coordinates coordinates, int searchRadius,
        int maximumRecords, IEnumerable<string> accessNeeds, bool ignoreCache,
        SiteSupportsServiceFilter siteSupportsServiceFilter = null);

    Task<Site> GetSiteByIdAsync(string siteId, string scope = "*");
    Task<IEnumerable<SitePreview>> GetSitesPreview(bool includeDeleted = false);
    Task<IEnumerable<Site>> GetAllSites(bool includeDeleted = false, bool ignoreCache = false);
    Task<OperationResult> UpdateAccessibilities(string siteId, IEnumerable<Accessibility> accessibilities);
    Task<OperationResult> UpdateInformationForCitizens(string siteId, string informationForCitizens);

    Task<OperationResult> UpdateSiteDetailsAsync(string siteId, string name, string address, string phoneNumber,
        decimal? longitude, decimal? latitude);

    Task<OperationResult> UpdateSiteReferenceDetailsAsync(string siteId, string odsCode, string icb, string region);

    Task<OperationResult> SaveSiteAsync(string siteId, string odsCode, string name, string address, string phoneNumber,
        string icb, string region, Location location, IEnumerable<Accessibility> accessibilities, string type,
        SiteStatus? siteStatus = null);

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
    private static readonly SemaphoreSlim _siteCacheLock = new(1, 1);

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _siteSupportsServiceCacheLocks = new();

    private TimeSpan SiteSupportsServiceAbsoluteExpiration => TimeSpan.FromMinutes(60);
    private TimeSpan SiteSupportsServiceSlideThreshold => TimeSpan.FromMinutes(15);

    public async Task<IEnumerable<SiteWithDistance>> FindSitesByArea(Coordinates coordinates, int searchRadius,
        int maximumRecords, IEnumerable<string> accessNeeds, bool ignoreCache = false,
        SiteSupportsServiceFilter siteSupportsServiceFilter = null)
    {
        var accessibilityIds = accessNeeds.Where(an => string.IsNullOrEmpty(an) == false)
            .Select(an => $"accessibility/{an}").ToList();

        var sites = await GetAllSites(false, ignoreCache);

        if (await featureToggleHelper.IsFeatureEnabled(Flags.SiteStatus))
        {
            sites = sites.Where(s => s.status is SiteStatus.Online or null);
        }

        var sitesWithDistance = sites
            .Select(site => new SiteWithDistance(site,
                GeographyCalculations.CalculateDistanceInMetres(coordinates, site.Coordinates)));

        Func<SiteWithDistance, bool> filterPredicate = accessibilityIds.Any()
            ? s =>
                accessibilityIds.All(acc => s.Site.Accessibilities.SingleOrDefault(a => a.Id == acc)?.Value == "true")
            : s => true;

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

    private async Task<IEnumerable<SiteWithDistance>> GetSitesSupportingService(IEnumerable<SiteWithDistance> sites,
        string service, DateOnly from, DateOnly to,
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
                var siteOffersServiceDuringPeriod =
                    await GetSiteSupportingServiceInRange(swd.Site.Id, service, from, to, false);
                
                ArgumentNullException.ThrowIfNull(siteOffersServiceDuringPeriod);
                
                if (siteOffersServiceDuringPeriod.Value)
                {
                    concurrentBatchResults.Add(swd);
                }
            }).ToArray();

            await Task.WhenAll(siteOffersServiceDuringPeriodTasks);

            //the concurrentBatchResults lose their original order, so we need to order the end result
            results.AddRange(concurrentBatchResults.OrderBy(site => site.Distance).Take(maxRecords));
            iterations++;
        }

        logger.LogInformation(
            "GetSitesSupportingService returned {resultCount} result(s) after {iterationCount} iteration(s) for service '{service}'",
            results.Count, iterations, service);

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
        var allSites = await GetSitesFromStoreOrCache(ignoreCache);

        return includeDeleted ? allSites : allSites.Where(s => s.isDeleted is null or false);
    }

    public async Task<IEnumerable<SitePreview>> GetSitesPreview(bool includeDeleted = false)
    {
        var sites = await GetAllSites(includeDeleted);

        return sites.Select(s => new SitePreview(s.Id, s.Name, s.OdsCode, s.IntegratedCareBoard));
    }

    public async Task<OperationResult> SaveSiteAsync(string siteId, string odsCode, string name, string address,
        string phoneNumber, string icb,
        string region, Location location, IEnumerable<Accessibility> accessibilities, string type,
        SiteStatus? siteStatus = null)
    {
        var result = await siteStore.SaveSiteAsync(
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

        if (result.Success)
        {
            await UpdateSiteInCacheAsync(siteId);
        }

        return result;
    }

    public async Task<OperationResult> UpdateAccessibilities(string siteId, IEnumerable<Accessibility> accessibilities)
    {
        var result = await siteStore.UpdateAccessibilities(siteId, accessibilities);
        if (result.Success)
        {
            await UpdateSiteInCacheAsync(siteId);
        }

        return result;
    }

    public async Task<OperationResult> UpdateInformationForCitizens(string siteId, string informationForCitizens)
    {
        var result = await siteStore.UpdateInformationForCitizens(siteId, informationForCitizens);
        if (result.Success)
        {
            await UpdateSiteInCacheAsync(siteId);
        }

        return result;
    }

    public async Task<OperationResult> UpdateSiteDetailsAsync(string siteId, string name, string address,
        string phoneNumber,
        decimal? longitude, decimal? latitude)
    {
        var result = await siteStore.UpdateSiteDetails(siteId, name, address, phoneNumber, longitude, latitude);
        if (result.Success)
        {
            await UpdateSiteInCacheAsync(siteId);
        }

        return result;
    }

    public async Task<OperationResult> UpdateSiteReferenceDetailsAsync(string siteId, string odsCode, string icb,
        string region)
    {
        var result = await siteStore.UpdateSiteReferenceDetails(siteId, odsCode, icb, region);
        if (result.Success)
        {
            await UpdateSiteInCacheAsync(siteId);
        }

        return result;
    }

    public async Task<OperationResult> SetSiteStatus(string siteId, SiteStatus status)
    {
        var result = await siteStore.UpdateSiteStatusAsync(siteId, status);
        if (result.Success)
        {
            await UpdateSiteInCacheAsync(siteId);
        }

        return result;
    }

    public async Task<IEnumerable<Site>> GetSitesInIcbAsync(string icb)
        => await siteStore.GetSitesInIcbAsync(icb);

    public async Task<OperationResult> ToggleSiteSoftDeletionAsync(string siteId)
    {
        var result = await siteStore.ToggleSiteSoftDeletionAsync(siteId);
        if (result.Success)
        {
            await UpdateSiteInCacheAsync(siteId);
        }

        return result;
    }

    public async Task<IEnumerable<SiteWithDistance>> QuerySitesAsync(SiteFilter[] filters, int maxRecords,
        bool ignoreCache)
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
            var filterCoordinates = new Coordinates { Latitude = filter.Latitude, Longitude = filter.Longitude };

            var sitesWithDistance = filteredSites
                .Select(site =>
                    new SiteWithDistance(site,
                        GeographyCalculations.CalculateDistanceInMetres(filterCoordinates, site.Coordinates)))
                .ToList();

            if (filter.Availability is not null && filter.Availability.Services?.Length > 0)
            {
                // Adding .Single() here as the current implementation only allows for filtering on a single service
                // This will need updating if we decide to allow filtering on multiple services
                var siteSupportsServiceFilter = new SiteSupportsServiceFilter(filter.Availability.Services.Single(),
                    filter.Availability.From!.Value, filter.Availability.Until!.Value);

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
            accessibilityIds =
            [
                .. filter.AccessNeeds
                    .Where(an => !string.IsNullOrEmpty(an))
                    .Select(an => $"accessibility/{an}")
            ];
        }

        return site =>
        {
            if (hasAccessNeedsFilter)
            {
                if (!accessibilityIds.All(acc =>
                        site.Accessibilities.SingleOrDefault(a => a.Id == acc)?.Value == "true"))
                {
                    return false;
                }
            }

            if (hasSiteTypeFilter)
            {
                var (includedTypes, excludedTypes) = ParseSiteTypeFilters(filter.Types);

                if (includedTypes.Count > 0 &&
                    !includedTypes.Any(st => st.Equals(site.Type, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return false;
                }

                if (excludedTypes.Count > 0 &&
                    excludedTypes.Any(st => st.Equals(site.Type, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(filter.OdsCode))
            {
                return filter.OdsCode.Equals(site.OdsCode, StringComparison.InvariantCultureIgnoreCase);
            }

            var distance = GeographyCalculations.CalculateDistanceInMetres(filter.Coordinates, site.Coordinates);
            return distance <= filter.SearchRadius;
        };
    }

    private static (List<string> IncludedSiteTypes, List<string> ExcludedSiteTypes) ParseSiteTypeFilters(
        string[] siteTypes)
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
                .OrderBy(x =>
                    x.Filter.Priority.HasValue ? 0 : 1) // Any filter with the priority prop set moves to the front
                .ThenBy(x => x.Filter.Priority ?? int.MaxValue) // Then order by prioirty ascending
                .ThenBy(x => x.Index) // Then order by index ascending if any filters have no priority set
                .Select(x => x.Filter)
                .ToList()
            : filters;
    }

    private async Task<IEnumerable<Site>> GetSitesFromStoreOrCache(bool ignoreCache = false)
    {
        if (ignoreCache || options.Value.DisableSiteCache)
        {
            return await siteStore.GetAllSites();
        }

        var sitesFromCache = memoryCache.Get(options.Value.SiteCacheKey) as IEnumerable<Site>;
        if (sitesFromCache != null)
        {
            return sitesFromCache;
        }

        var sites = (await siteStore.GetAllSites()).ToList();
        memoryCache.Set(options.Value.SiteCacheKey, sites,
            time.GetUtcNow().AddMinutes(options.Value.SiteCacheDuration));
        return sites;
    }

    internal async Task UpdateSiteInCacheAsync(string siteId)
    {
        if (options.Value.DisableSiteCache)
        {
            return;
        }

        await _siteCacheLock.WaitAsync();
        try
        {
            if (!memoryCache.TryGetValue(options.Value.SiteCacheKey, out List<Site> cachedSites))
            {
                return;
            }

            var updatedSite = await siteStore.GetSiteById(siteId);

            var existingIndex = cachedSites.FindIndex(s => s.Id == siteId);

            if (existingIndex >= 0)
            {
                if (updatedSite != null)
                {
                    cachedSites[existingIndex] = updatedSite;
                }
                else
                {
                    cachedSites.RemoveAt(existingIndex);
                }
            }
            else if (updatedSite != null)
            {
                cachedSites.Add(updatedSite);
            }

            memoryCache.Set(options.Value.SiteCacheKey, cachedSites,
                time.GetUtcNow().AddMinutes(options.Value.SiteCacheDuration));
        }
        finally
        {
            _siteCacheLock.Release();
        }
    }

    private async Task<bool?> GetSiteSupportingServiceInRange(string siteId, string service, DateOnly from,
        DateOnly until, bool isSlide)
    {
        var utcNow = time.GetUtcNow();
        var currentHoursAndMinutes = HourAndMinutes(utcNow.DateTime);
        var cacheKey = GetCacheSiteServiceSupportDateRangeKey(siteId, service, from, until);
        
        //should this be created for both code paths??
        var siteSupportsServiceCacheKeyLock =
            _siteSupportsServiceCacheLocks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));

        if (isSlide)
        {
            //a lock exists, and we are unwilling to wait any amount of time for it to be released
            //this means another request is already sliding this cache value
            if (!await siteSupportsServiceCacheKeyLock.WaitAsync(0))
            {
                siteSupportsServiceCacheKeyLock.Release();
                _siteSupportsServiceCacheLocks.TryRemove(cacheKey, out _);
                
                //no need to invoke the slide as another thread is performing the action concurrently
                return null;
            }
        }
        else
        {
            //we want to wait until the lock is free'd before
            await siteSupportsServiceCacheKeyLock.WaitAsync();
            
            if (memoryCache.TryGetValue<SiteSupportServiceCache>(cacheKey, out var siteSupportsService))
            {
                ArgumentNullException.ThrowIfNull(siteSupportsService);
                
                //check if we want to update the existing cache in the background lazily...
                if (siteSupportsService.TimeUpdated.Add(SiteSupportsServiceSlideThreshold) < currentHoursAndMinutes)
                {
                    //Sliding cache functionality

                    //Update the cache value so the NEXT request gets a newer version of the latest DB value
                    //This approach means the cache entry is never guaranteed to be the exact latest DB value (unless a cache value does not exist) - but it is recent enough to not have a big impact
                    //The performance gain is a sufficient benefit to the value being potentially slightly behind the current DB state
                    _ = GetSiteSupportingServiceInRange(siteId, service, from, until, isSlide: true);
                }
                
                siteSupportsServiceCacheKeyLock.Release();
                _siteSupportsServiceCacheLocks.TryRemove(cacheKey, out _);

                //return the current cached value regardless of whether sliding was invoked
                return siteSupportsService!.Value;
            }
        }

        var siteOffersServiceDuringPeriod = await FetchSiteOffersServiceDuringPeriod(siteId, service, from, until);
        memoryCache.Set(cacheKey, new SiteSupportServiceCache(siteOffersServiceDuringPeriod, currentHoursAndMinutes),
                utcNow.Add(SiteSupportsServiceAbsoluteExpiration));

        siteSupportsServiceCacheKeyLock.Release();
        _siteSupportsServiceCacheLocks.TryRemove(cacheKey, out _);
            
        return siteOffersServiceDuringPeriod;
    }

    //TODO just use UTC datetime???
    //use a minimal version of TimeOnly to keep the cache object as small as possible
    private static TimeOnly HourAndMinutes(DateTime dateTime)
    {
        return new TimeOnly(dateTime.Hour, dateTime.Minute);
    }

    private async Task<bool> FetchSiteOffersServiceDuringPeriod(string siteId, string service, DateOnly from,
        DateOnly until)
    {
        var dateStringsInRange = GetDateStringsInRange(from, until);
        return await availabilityStore.SiteOffersServiceDuringPeriod(siteId, service, dateStringsInRange);
    }

    private string GetCacheSiteServiceSupportDateRangeKey(string siteId, string service, DateOnly from, DateOnly until)
    {
        var dateRange = $"{from.ToString("yyyyMMdd")}_{until.ToString("yyyyMMdd")}";
        return $"site_{siteId}_supports_{service}_in_{dateRange}";
    }

    private record SiteSupportServiceCache(bool Value, TimeOnly TimeUpdated);
}
