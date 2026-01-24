namespace Nhs.Appointments.Core.Sites;

public class SiteServiceOptions
{
    /// <summary>
    ///     When true, disables the site cache altogether. The E2E tests rely on creating and destroying sites during their
    ///     setup and teardown routines, so we need to disable the cache whilst doing this.
    /// </summary>
    public bool DisableSiteCache { get; set; }

    /// <summary>
    ///     The duration, in minutes, to cache sites for.
    /// </summary>
    public int SiteCacheDuration { get; set; }

    /// <summary>
    ///     The unique string identifying the site cache.
    /// </summary>
    public string SiteCacheKey { get; set; }
    
    /// <summary>
    ///     The duration, in seconds, that a SiteSupportsService 'SlidingCache' value will expire absolutely after created date (if no slide occurs during its lifespan and updates itself)
    /// </summary>
    public int SiteSupportsServiceSlidingCacheAbsoluteExpirationSeconds { get; set; }
    
    /// <summary>
    ///     The duration, in seconds, for the SiteSupportsService 'SlidingCache' SLIDE_THRESHOLD value
    ///     A slide WILL NOT occur if a cache value is requested less than this timespan. i.e if utcNow '<' cacheValueCreatedDate.addSeconds(SLIDE_THRESHOLD)
    ///     A slide WILL occur if a cache value is requested greater than this timespan. i.e if utcNow '>=' cacheValueCreatedDate.addSeconds(SLIDE_THRESHOLD)
    /// </summary>
    public int SiteSupportsServiceSlidingCacheSlideThresholdSeconds { get; set; }
    
    /// <summary>
    /// When fetching batched results to build up the max site records, what should the batch size be as a multiplier of the maxRecords required.
    /// i.e. when SiteSupportsServiceBatchMultiplier = 2, and maxRecords = 50 - then service will fetch data in batches of 100, until 50 valid sites are returned.
    /// </summary>
    public int SiteSupportsServiceBatchMultiplier { get; set; }
}
