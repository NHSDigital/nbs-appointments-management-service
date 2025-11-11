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
            ["DISABLE_SITE_CACHE"] = "true"
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
    }
}
