using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Core.Sites;

namespace Nhs.Appointments.Core.UnitTests;

public class ServiceRegistrationTests
{
    [Fact]
    public void ConfigureSiteService_WithConfigurationValues_ShouldBindOptionsCorrectly()
    {
        var services = new ServiceCollection();
        var configurationData = new Dictionary<string, string>
        {
            ["SITE_CACHE_KEY"] = "test-sites-key",
            ["SITE_CACHE_DURATION_MINUTES"] = "15",
            ["DISABLE_SITE_CACHE"] = "true",
            ["SITE_SUPPORTS_SERVICE_SLIDING_CACHE_ABSOLUTE_EXPIRATION_SECONDS"] = "5",
            ["SITE_SUPPORTS_SERVICE_SLIDING_CACHE_SLIDE_THRESHOLD_SECONDS"] = "1",
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
            .Build();

        services.ConfigureSiteService(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<SiteServiceOptions>>().Value;

        options.SiteCacheKey.Should().Be("test-sites-key");
        options.SiteCacheDuration.Should().Be(15);
        options.DisableSiteCache.Should().BeTrue();
        options.SiteSupportsServiceSlidingCacheSlideThresholdSeconds.Should().Be(1);          
        options.SiteSupportsServiceSlidingCacheAbsoluteExpirationSeconds.Should().Be(5);
    }

    [Fact]
    public void ConfigureSiteService_WithMissingConfigurationValues_ShouldUseDefaults()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>())
            .Build();

        services.ConfigureSiteService(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<SiteServiceOptions>>().Value;

        options.SiteCacheKey.Should().Be("sites");
        options.SiteCacheDuration.Should().Be(10);
        options.DisableSiteCache.Should().BeFalse();
        options.SiteSupportsServiceSlidingCacheSlideThresholdSeconds.Should().Be(900);
        options.SiteSupportsServiceSlidingCacheAbsoluteExpirationSeconds.Should().Be(14400);
    }
}
