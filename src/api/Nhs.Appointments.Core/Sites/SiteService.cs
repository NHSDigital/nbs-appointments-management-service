using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Caching;
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
    ILogger<ISiteService> logger,
    IFeatureToggleHelper featureToggleHelper,
    ICacheService cacheService,
    IOptions<SiteServiceOptions> options) : ISiteService
{
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
            siteSupportsServiceFilter.services,
            siteSupportsServiceFilter.from,
            siteSupportsServiceFilter.until,
            maximumRecords,
            ignoreCache);
    }

    private async Task<IEnumerable<SiteWithDistance>> GetSitesSupportingService(IEnumerable<SiteWithDistance> sites,
        List<string> services,
        DateOnly from, DateOnly to,
        int maxRecords, bool ignoreCache = false)
    {
        var orderedSites = sites.OrderBy(site => site.Distance).ToList();
        var uniqueSortedServices = services.OrderBy(s => s).Distinct().ToList();

        var results = new List<SiteWithDistance>();

        var iterations = 0;
        
        var batchSize = maxRecords * options.Value.SiteSupportsServiceBatchMultiplier;

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
                if (ignoreCache)
                {
                    var siteOffersServiceDuringPeriod = await FetchSiteOffersServiceDuringPeriod(swd.Site.Id, uniqueSortedServices, from, to);
                    
                    ArgumentNullException.ThrowIfNull(siteOffersServiceDuringPeriod);

                    if (siteOffersServiceDuringPeriod)
                    {
                        concurrentBatchResults.Add(swd);
                    }
                }
                else
                {
                    var cacheKey = GetCacheSiteServiceSupportDateRangeKey(swd.Site.Id, uniqueSortedServices, from, to);
                    var slideThreshold =
                        TimeSpan.FromSeconds(options.Value.SiteSupportsServiceSlidingCacheSlideThresholdSeconds);
                    var slideExpiry = TimeSpan.FromSeconds(options.Value
                        .SiteSupportsServiceSlidingCacheAbsoluteExpirationSeconds);
                
                    var siteOffersServiceDuringPeriodLazyCache =
                        await cacheService.GetLazySlidingCacheValue(cacheKey,
                            new LazySlideCacheOptions<bool>(
                                async () => await FetchSiteOffersServiceDuringPeriod(swd.Site.Id, uniqueSortedServices,
                                    from, to), slideThreshold, slideExpiry));

                    ArgumentNullException.ThrowIfNull(siteOffersServiceDuringPeriodLazyCache);
                    if (siteOffersServiceDuringPeriodLazyCache)
                    {
                        concurrentBatchResults.Add(swd);
                    }
                }
            }).ToArray();

            await Task.WhenAll(siteOffersServiceDuringPeriodTasks);

            //the concurrentBatchResults lose their original order, so we need to order the end result
            results.AddRange(concurrentBatchResults.OrderBy(site => site.Distance).Take(maxRecords));
            iterations++;
        }

        logger.LogInformation(
            "GetSitesSupportingService returned {resultCount} result(s) after {iterationCount} iteration(s) for services '{services}'",
            results.Count, iterations, uniqueSortedServices);

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
        return await siteStore.SaveSiteAsync(
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
    }

    public async Task<OperationResult> UpdateAccessibilities(string siteId, IEnumerable<Accessibility> accessibilities)
    {
        return await siteStore.UpdateAccessibilities(siteId, accessibilities);
    }

    public async Task<OperationResult> UpdateInformationForCitizens(string siteId, string informationForCitizens)
    {
        return await siteStore.UpdateInformationForCitizens(siteId, informationForCitizens);
    }

    public async Task<OperationResult> UpdateSiteDetailsAsync(string siteId, string name, string address,
        string phoneNumber,
        decimal? longitude, decimal? latitude)
    {
        return await siteStore.UpdateSiteDetails(siteId, name, address, phoneNumber, longitude, latitude);
    }

    public async Task<OperationResult> UpdateSiteReferenceDetailsAsync(string siteId, string odsCode, string icb,
        string region)
    {
        return await siteStore.UpdateSiteReferenceDetails(siteId, odsCode, icb, region);
    }

    public async Task<OperationResult> SetSiteStatus(string siteId, SiteStatus status)
    {
        return await siteStore.UpdateSiteStatusAsync(siteId, status);
    }

    public async Task<IEnumerable<Site>> GetSitesInIcbAsync(string icb)
        => await siteStore.GetSitesInIcbAsync(icb);

    public async Task<OperationResult> ToggleSiteSoftDeletionAsync(string siteId)
    {
        return await siteStore.ToggleSiteSoftDeletionAsync(siteId);
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
                var siteSupportsServiceFilter = new SiteSupportsServiceFilter([.. filter.Availability.Services],
                    filter.Availability.From!.Value, filter.Availability.Until!.Value);

                var serviceResults = await GetSitesSupportingService(
                    sitesWithDistance,
                    siteSupportsServiceFilter.services,
                    siteSupportsServiceFilter.from,
                    siteSupportsServiceFilter.until,
                    maxRecords: maxRecords,
                    ignoreCache: ignoreCache);

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

        if (options.Value.SlidingCacheEnabled)
        {
            return await cacheService.GetLazySlidingCacheValue(
                options.Value.SiteCacheKey,
                new LazySlideCacheOptions<IEnumerable<Site>>(
                    async () => await siteStore.GetAllSites(),
                    TimeSpan.FromMinutes(options.Value.SiteSlideCacheDuration),
                    TimeSpan.FromMinutes(options.Value.SiteCacheDuration)));
        }

        return await cacheService.GetCacheValue(
            options.Value.SiteCacheKey,
            new CacheOptions<IEnumerable<Site>>(
                async () => await siteStore.GetAllSites(),
                TimeSpan.FromMinutes(options.Value.SiteCacheDuration)));
    }

    private static string GetCacheSiteServiceSupportDateRangeKey(string siteId, List<string> services, DateOnly from,
        DateOnly until)
    {
        var joinedServices = string.Join('_', services);
        var dateRange = $"{from:yyyyMMdd}_{until:yyyyMMdd}";
        return $"site_{siteId}_supports_{joinedServices}_in_{dateRange}";
    }

    private async Task<bool> FetchSiteOffersServiceDuringPeriod(string siteId, List<string> services, DateOnly from,
        DateOnly until)
    {
        logger.LogInformation(
            "FetchSiteOffersServiceDuringPeriod invocated for site: '{siteId}', services: '{services}', from: '{from}', until: '{until}'",
            siteId, string.Join('_', services), from.ToString("yyyyMMdd"), until.ToString("yyyyMMdd"));
        
        var dateStringsInRange = GetDateStringsInRange(from, until);
        return await availabilityStore.SiteSupportsAllServicesOnSingleDateInRangeAsync(siteId, services,
            dateStringsInRange);
    }
}
