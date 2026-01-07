using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nhs.Appointments.Core.Inspectors;
using Nhs.Appointments.Core.Sites;

namespace Nhs.Appointments.Core;

public static class ServiceRegistration
{
    public static IServiceCollection AddRequestInspectors(this IServiceCollection services)
    {
        var inspectorTypes = typeof(IRequestInspector).Assembly
            .GetTypes()
            .Where(t => typeof(IRequestInspector).IsAssignableFrom(t) && t.IsClass && t.IsAbstract == false);

        foreach (var type in inspectorTypes)
        {
            services.AddSingleton(type);
        }

        return services;
    }

    public static IServiceCollection ConfigureSiteService(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SiteServiceOptions>(opts =>
        {
            opts.SiteCacheKey = configuration.GetValue("SITE_CACHE_KEY", "sites");
            opts.SiteCacheDuration = configuration.GetValue("SITE_CACHE_DURATION_MINUTES", 10);
            opts.DisableSiteCache = configuration.GetValue("DISABLE_SITE_CACHE", false);
            //default 4 hours
            opts.SiteSupportsServiceSlidingCacheAbsoluteExpirationSeconds = configuration.GetValue("SITE_SUPPORTS_SERVICE_SLIDING_CACHE_ABSOLUTE_EXPIRATION_SECONDS", 14400);
            //default 15 mins
            opts.SiteSupportsServiceSlidingCacheSlideThresholdSeconds = configuration.GetValue("SITE_SUPPORTS_SERVICE_SLIDING_CACHE_SLIDE_THRESHOLD_SECONDS", 900);
        });

        return services;
    }
}
