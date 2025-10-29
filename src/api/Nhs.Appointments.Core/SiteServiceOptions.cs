namespace Nhs.Appointments.Core;

public class SiteServiceOptions
{
    /// <summary>
    ///     When true, disables the site cache altogether. The E2E tests rely on creating and destroying sites during their
    ///     setup and teardown routines, so we need to disable the cache whilst doing this.
    /// </summary>
    public bool DisableSiteCache { get; set; } = false;

    /// <summary>
    ///     The duration, in minutes, to cache sites for.
    /// </summary>
    public int SiteCacheDuration { get; set; } = 10;

    /// <summary>
    ///     The unique string identifying the site cache.
    /// </summary>
    public string SiteCacheKey { get; set; } = "sites";
}
