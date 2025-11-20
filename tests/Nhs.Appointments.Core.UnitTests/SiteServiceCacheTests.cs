using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Geography;
using Nhs.Appointments.Core.Sites;

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
            TimeProvider.System, _featureToggleHelper.Object, _options.Object, new GeographyService());
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

    [Fact]
    public async Task UpdateSiteInCache_UpdatesExistingSite_WhenSiteExists()
    {
        var siteId = "00f5bcca-a952-41f0-885b-e100685c5324";
        var oldSite = SiteServiceCacheTestsMockData.CreateMockSite(siteId, "Old Site Name");
        var updatedSite = SiteServiceCacheTestsMockData.CreateMockSite(siteId, "Updated Site Name");

        var cachedSites = new List<Site> { oldSite };
        _memoryCache.Set("sites", cachedSites);

        _siteStore.Setup(x => x.GetSiteById(siteId))
            .ReturnsAsync(updatedSite);

        await _sut.UpdateSiteInCacheAsync(siteId);

        var cachedResult = _memoryCache.Get<List<Site>>("sites");
        cachedResult.Single().Name.Should().Be("Updated Site Name");
    }

    [Fact]
    public async Task UpdateSiteInCache_AddsNewSite_WhenSiteDoesNotExist()
    {
        var existingSite =
            SiteServiceCacheTestsMockData.CreateMockSite("a42e8ff4-97a3-47b2-a9f7-a6f8f9829765", "Existing Site Name");
        var newSite =
            SiteServiceCacheTestsMockData.CreateMockSite("d8da1228-b953-4079-938b-33686b8ce580", "New Site Name");

        var cachedSites = new List<Site> { existingSite };
        _memoryCache.Set("sites", cachedSites);

        _siteStore.Setup(x => x.GetSiteById(existingSite.Id))
            .ReturnsAsync(existingSite);
        _siteStore.Setup(x => x.GetSiteById(newSite.Id))
            .ReturnsAsync(newSite);

        await _sut.UpdateSiteInCacheAsync(newSite.Id);

        var cachedResult = _memoryCache.Get<List<Site>>("sites");
        cachedResult.Should().HaveCount(2);
        cachedResult.Should().Contain(site => site.Id == existingSite.Id);
        cachedResult.Should().Contain(site => site.Id == newSite.Id);
    }


    [Fact]
    public async Task UpdateSiteInCache_RemovesSite_WhenSiteNoLongerExists()
    {
        var siteOne =
            SiteServiceCacheTestsMockData.CreateMockSite("a42e8ff4-97a3-47b2-a9f7-a6f8f9829765", "Site One");
        var siteTwo =
            SiteServiceCacheTestsMockData.CreateMockSite("d8da1228-b953-4079-938b-33686b8ce580", "Site Two");

        var cachedSites = new List<Site> { siteOne, siteTwo };
        _memoryCache.Set("sites", cachedSites);

        _siteStore.Setup(x => x.GetSiteById(siteOne.Id))
            .ReturnsAsync((Site)null);
        _siteStore.Setup(x => x.GetSiteById(siteTwo.Id))
            .ReturnsAsync(siteTwo);

        await _sut.UpdateSiteInCacheAsync(siteOne.Id);

        var cachedResult = _memoryCache.Get<List<Site>>("sites");
        cachedResult.Should().HaveCount(1);
        cachedResult.Should().NotContain(site => site.Id == siteOne.Id);
        cachedResult.Should().Contain(site => site.Id == siteTwo.Id);
    }

    [Fact]
    public async Task UpdateSiteInCache_DoesNothing_WhenCacheDisabled()
    {
        var disabledOptions = Options.Create(new SiteServiceOptions
        {
            SiteCacheKey = "AllSites", SiteCacheDuration = 60, DisableSiteCache = true
        });

        var _service = new SiteService(_siteStore.Object, _availabilityStore.Object, _memoryCache, _logger.Object,
            TimeProvider.System, _featureToggleHelper.Object, disabledOptions, new GeographyService());

        await _service.UpdateSiteInCacheAsync("ab648be7-f10d-4c5d-a534-fec12b61f998");

        _siteStore.Verify(x => x.GetSiteById("ab648be7-f10d-4c5d-a534-fec12b61f998"), Times.Never);
    }


    [Fact]
    public async Task UpdateSiteInCache_DoesNothing_WhenCacheNotInitialized()
    {
        await _sut.UpdateSiteInCacheAsync("ab648be7-f10d-4c5d-a534-fec12b61f998");

        _siteStore.Verify(x => x.GetSiteById(It.IsAny<string>()), Times.Never);
        _memoryCache.Get("sites").Should().BeNull();
    }

    [Fact]
    public async Task UpdateSiteInCache_HandlesParallelUpdates_WithoutRaceConditions()
    {
        var siteOne = SiteServiceCacheTestsMockData.CreateMockSite("40141575-470e-4587-b250-7251b9470bc8", "Site 1");
        var siteTwo = SiteServiceCacheTestsMockData.CreateMockSite("212e33d9-d28e-419b-8267-b58543500af4", "Site 2");
        var siteThree = SiteServiceCacheTestsMockData.CreateMockSite("4e4ad172-e81f-490b-b4d9-f85149ccf145", "Site 3");

        var cachedSites = new List<Site> { siteOne, siteTwo, siteThree };
        _memoryCache.Set("sites", cachedSites);

        var updatedSiteOne = SiteServiceCacheTestsMockData.CreateMockSite(siteOne.Id, "Updated Site 1");
        var updatedSiteTwo = SiteServiceCacheTestsMockData.CreateMockSite(siteTwo.Id, "Updated Site 2");
        var updatedSiteThree = SiteServiceCacheTestsMockData.CreateMockSite(siteThree.Id, "Updated Site 3");

        _siteStore.Setup(x => x.GetSiteById(siteOne.Id)).ReturnsAsync(updatedSiteOne);
        _siteStore.Setup(x => x.GetSiteById(siteTwo.Id)).ReturnsAsync(updatedSiteTwo);
        _siteStore.Setup(x => x.GetSiteById(siteThree.Id)).ReturnsAsync(updatedSiteThree);

        var tasks = new[]
        {
            Task.Run(() => _sut.UpdateSiteInCacheAsync(siteOne.Id)),
            Task.Run(() => _sut.UpdateSiteInCacheAsync(siteTwo.Id)),
            Task.Run(() => _sut.UpdateSiteInCacheAsync(siteThree.Id))
        };

        await Task.WhenAll(tasks);

        var cachedResult = _memoryCache.Get<List<Site>>("sites");
        cachedResult.Should().HaveCount(3);
        cachedResult.Should().Contain(site => site.Id == siteOne.Id);
        cachedResult.Should().Contain(site => site.Id == siteTwo.Id);
        cachedResult.Should().Contain(site => site.Id == siteThree.Id);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SaveSiteAsync_UpdatesTheCache(bool operationSuccessful)
    {
        _siteStore.Setup(x =>
                x.SaveSiteAsync(
                    "f4c231eb-aff5-418b-9ea0-8ccb39a83b68",
                    "ODS123",
                    "Site Name",
                    "Site Address",
                    "0113 2345678",
                    "ICB123",
                    "R1",
                    It.IsAny<Location>(),
                    new List<Accessibility>(),
                    "GP Pharmacy",
                    SiteStatus.Online,
                    null))
            .ReturnsAsync(new OperationResult(operationSuccessful));

        _memoryCache.Set("sites", new List<Site>());

        await _sut.SaveSiteAsync(
            "f4c231eb-aff5-418b-9ea0-8ccb39a83b68",
            "ODS123",
            "Site Name",
            "Site Address",
            "0113 2345678",
            "ICB123",
            "R1",
            new Location("Point", [.507, 65]),
            new List<Accessibility>(),
            "GP Pharmacy",
            SiteStatus.Online);

        if (operationSuccessful)
        {
            _siteStore.Verify(x => x.GetSiteById(It.IsAny<string>()), Times.Once);
        }
        else
        {
            _siteStore.Verify(x => x.GetSiteById(It.IsAny<string>()), Times.Never);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateAccessibilities_UpdatesTheCache(bool operationSuccessful)
    {
        _siteStore.Setup(x =>
                x.UpdateAccessibilities("f4c231eb-aff5-418b-9ea0-8ccb39a83b68",
                    new List<Accessibility>()))
            .ReturnsAsync(new OperationResult(operationSuccessful));

        _memoryCache.Set("sites", new List<Site>());

        await _sut.UpdateAccessibilities("f4c231eb-aff5-418b-9ea0-8ccb39a83b68",
            new List<Accessibility>());

        if (operationSuccessful)
        {
            _siteStore.Verify(x => x.GetSiteById(It.IsAny<string>()), Times.Once);
        }
        else
        {
            _siteStore.Verify(x => x.GetSiteById(It.IsAny<string>()), Times.Never);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateInformationForCitizens_UpdatesTheCache(bool operationSuccessful)
    {
        _siteStore.Setup(x =>
                x.UpdateInformationForCitizens("f4c231eb-aff5-418b-9ea0-8ccb39a83b68",
                    "Info for citizens"))
            .ReturnsAsync(new OperationResult(operationSuccessful));

        _memoryCache.Set("sites", new List<Site>());

        await _sut.UpdateInformationForCitizens("f4c231eb-aff5-418b-9ea0-8ccb39a83b68",
            "Info for citizens");

        if (operationSuccessful)
        {
            _siteStore.Verify(x => x.GetSiteById(It.IsAny<string>()), Times.Once);
        }
        else
        {
            _siteStore.Verify(x => x.GetSiteById(It.IsAny<string>()), Times.Never);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateSiteDetailsAsync_UpdatesTheCache(bool operationSuccessful)
    {
        _siteStore.Setup(x =>
                x.UpdateSiteDetails("f4c231eb-aff5-418b-9ea0-8ccb39a83b68",
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null, null))
            .ReturnsAsync(new OperationResult(operationSuccessful));

        _memoryCache.Set("sites", new List<Site>());

        await _sut.UpdateSiteDetailsAsync("f4c231eb-aff5-418b-9ea0-8ccb39a83b68", "Site name", "Address",
            "0113 123 455", null, null);

        if (operationSuccessful)
        {
            _siteStore.Verify(x => x.GetSiteById(It.IsAny<string>()), Times.Once);
        }
        else
        {
            _siteStore.Verify(x => x.GetSiteById(It.IsAny<string>()), Times.Never);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateSiteReferenceDetailsAsync_UpdatesTheCache(bool operationSuccessful)
    {
        _siteStore.Setup(x =>
                x.UpdateSiteReferenceDetails("f4c231eb-aff5-418b-9ea0-8ccb39a83b68", It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new OperationResult(operationSuccessful));

        _memoryCache.Set("sites", new List<Site>());

        await _sut.UpdateSiteReferenceDetailsAsync("f4c231eb-aff5-418b-9ea0-8ccb39a83b68", "ODC", "ICB",
            "Region");

        if (operationSuccessful)
        {
            _siteStore.Verify(x => x.GetSiteById(It.IsAny<string>()), Times.Once);
        }
        else
        {
            _siteStore.Verify(x => x.GetSiteById(It.IsAny<string>()), Times.Never);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SetSiteStatus_UpdatesTheCache(bool operationSuccessful)
    {
        _siteStore.Setup(x => x.UpdateSiteStatusAsync(
                "f4c231eb-aff5-418b-9ea0-8ccb39a83b68", SiteStatus.Online))
            .ReturnsAsync(new OperationResult(operationSuccessful));

        _memoryCache.Set("sites", new List<Site>());

        await _sut.SetSiteStatus("f4c231eb-aff5-418b-9ea0-8ccb39a83b68", SiteStatus.Online);

        if (operationSuccessful)
        {
            _siteStore.Verify(x => x.GetSiteById(It.IsAny<string>()), Times.Once);
        }
        else
        {
            _siteStore.Verify(x => x.GetSiteById(It.IsAny<string>()), Times.Never);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ToggleSiteSoftDeletionAsync_UpdatesTheCache(bool operationSuccessful)
    {
        _siteStore.Setup(x => x.ToggleSiteSoftDeletionAsync(
                "f4c231eb-aff5-418b-9ea0-8ccb39a83b68"))
            .ReturnsAsync(new OperationResult(operationSuccessful));

        _memoryCache.Set("sites", new List<Site>());

        await _sut.ToggleSiteSoftDeletionAsync("f4c231eb-aff5-418b-9ea0-8ccb39a83b68");

        if (operationSuccessful)
        {
            _siteStore.Verify(x => x.GetSiteById(It.IsAny<string>()), Times.Once);
        }
        else
        {
            _siteStore.Verify(x => x.GetSiteById(It.IsAny<string>()), Times.Never);
        }
    }
}
