using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Core.UnitTests;

public class SiteServiceCacheTests
{
    private readonly IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());
    private readonly Mock<ISiteStore> _siteStore = new();
    private readonly Mock<IAvailabilityStore> _availabilityStore = new();
    private readonly Mock<ILogger<ISiteService>> _logger = new();
    private readonly Mock<IFeatureToggleHelper> _featureToggleHelper = new();
    private readonly SiteService _sut;
    private readonly Mock<IOptions<SiteServiceOptions>> _options = new();

    public SiteServiceCacheTests()
    {
        _siteStore.Setup(store => store.GetAllSites())
            .ReturnsAsync(SiteServiceCacheTestsMockData.MockOnlyNonDeletedSites);
        _siteStore.Setup(store => store.GetAllSites())
            .ReturnsAsync(SiteServiceCacheTestsMockData.MockAllSites);

        _options.Setup(x => x.Value).Returns(new SiteServiceOptions
        {
            DisableSiteCache = false, SiteCacheDuration = 10, SiteCacheKey = "sites"
        });
        _sut = new SiteService(_siteStore.Object, _availabilityStore.Object, _memoryCache, _logger.Object,
            TimeProvider.System, _featureToggleHelper.Object, _options.Object);
    }

    [Fact(DisplayName = "The Site Cache is used by default (when excluding deleted sites)")]
    public async Task TheSiteCacheIsUsed()
    {
        var result = await _sut.GetAllSites();
        result.Should().BeEquivalentTo(SiteServiceCacheTestsMockData.MockOnlyNonDeletedSites);
        _siteStore.Verify(store => store.GetAllSites(), Times.Once);

        // Fetch again and check the store has not been hit a 2nd time
        result = await _sut.GetAllSites();
        result.Should().BeEquivalentTo(SiteServiceCacheTestsMockData.MockOnlyNonDeletedSites);
        _siteStore.Verify(store => store.GetAllSites(), Times.Once);
    }

    [Fact(DisplayName = "The Site Cache is used by default (when including deleted sites)")]
    public async Task TheSiteCacheIsUsed__RequestingDeletedSites()
    {
        var result = await _sut.GetAllSites(true);
        result.Should().BeEquivalentTo(SiteServiceCacheTestsMockData.MockAllSites);
        _siteStore.Verify(store => store.GetAllSites(), Times.Once);

        // Fetch again and check the store has not been hit a 2nd time
        result = await _sut.GetAllSites(true);
        result.Should().BeEquivalentTo(SiteServiceCacheTestsMockData.MockAllSites);
        _siteStore.Verify(store => store.GetAllSites(), Times.Once);
    }

    [Fact(DisplayName = "The Site Cache can be ignored (when including deleted sites)")]
    public async Task TheSiteCacheCanBeIgnored()
    {
        var result = await _sut.GetAllSites(ignoreCache: true);
        result.Should().BeEquivalentTo(SiteServiceCacheTestsMockData.MockOnlyNonDeletedSites);
        _siteStore.Verify(store => store.GetAllSites(), Times.Once);

        // Fetch again and check the store has been a 2nd time
        result = await _sut.GetAllSites(ignoreCache: true);
        result.Should().BeEquivalentTo(SiteServiceCacheTestsMockData.MockOnlyNonDeletedSites);
        _siteStore.Verify(store => store.GetAllSites(), Times.Exactly(2));
    }

    [Fact(DisplayName = "The Site Cache can be ignored (when excluding deleted sites)")]
    public async Task TheSiteCacheCanBeIgnored__RequestingDeletedSites()
    {
        var result = await _sut.GetAllSites(ignoreCache: true, includeDeleted: true);
        result.Should().BeEquivalentTo(SiteServiceCacheTestsMockData.MockAllSites);
        _siteStore.Verify(store => store.GetAllSites(), Times.Once);

        // Fetch again and check the store has been a 2nd time
        result = await _sut.GetAllSites(ignoreCache: true, includeDeleted: true);
        result.Should().BeEquivalentTo(SiteServiceCacheTestsMockData.MockAllSites);
        _siteStore.Verify(store => store.GetAllSites(), Times.Exactly(2));
    }

    [Fact(DisplayName = "APPT-1586: Caches consistently regardless of the value of includeDeleted")]
    public async Task CachesConsistently()
    {
        // Step 1: fetch sites EXCLUDING deleted
        var result = await _sut.GetAllSites();
        result.Should().BeEquivalentTo(SiteServiceCacheTestsMockData.MockOnlyNonDeletedSites);

        // Step 2: fetch all sites INCLUDING deleted. Should use the cache from the first request
        // The bugged behaviour in APPT-1586 was that the initial request which excludes deleted sites was
        // being cached, then this cache was used when returning the full list. Hence the deleted sites were erroneously missing
        result = await _sut.GetAllSites(true);
        result.Should().BeEquivalentTo(SiteServiceCacheTestsMockData.MockAllSites);
        _siteStore.Verify(store => store.GetAllSites(), Times.Once);
    }
}
