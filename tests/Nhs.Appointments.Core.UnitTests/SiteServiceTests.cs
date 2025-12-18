using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Caching;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Geography;
using Nhs.Appointments.Core.Sites;

namespace Nhs.Appointments.Core.UnitTests;

public class SiteServiceTests
{
    private readonly Mock<ICacheEntry> _cacheEntry = new();
    private readonly Mock<IMemoryCache> _memoryCache = new();
    private readonly Mock<ISiteStore> _siteStore = new();
    private readonly Mock<IAvailabilityStore> _availabilityStore = new();
    private readonly Mock<ILogger<ISiteService>> _logger = new();
    private readonly Mock<IFeatureToggleHelper> _featureToggleHelper = new();
    private readonly SiteService _sut;
    private readonly Mock<IOptions<SiteServiceOptions>> _options = new();

    public SiteServiceTests()
    {
        _options.Setup(x => x.Value).Returns(new SiteServiceOptions
        {
            DisableSiteCache = false, SiteCacheDuration = 10, SiteCacheKey = "sites"
        });

        var cacheService = new CacheService(_memoryCache.Object, TimeProvider.System);
        
        _sut = new SiteService(_siteStore.Object, _availabilityStore.Object, _memoryCache.Object, _logger.Object,
            TimeProvider.System, _featureToggleHelper.Object, cacheService, _options.Object);
        _memoryCache.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(_cacheEntry.Object);
    }

    [Fact]
    public async Task FindSitesByArea_ReturnsSitesOrderedByDistance_InAscendingOrder()
    {
        var sites = new List<Site>
        {
            new(
                Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                Name: "Site 2",
                Address: "2 Park Row",
                PhoneNumber: "0113 1111111",
                OdsCode: "ABC02",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                location: new Location("Point", [.507, 65]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility>
                {
                    new Accessibility(Id: "accessibility/access_need_1", Value: "true")
                },
                status: SiteStatus.Online, isDeleted: null,
                Type: string.Empty),
            new(
                Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bc",
                Name: "Site 3",
                Address: "3 Park Row",
                PhoneNumber: "0113 1111111",
                OdsCode: "ABC03",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                location: new Location("Point", [.506, 65]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility>
                {
                    new Accessibility(Id: "accessibility/access_need_1", Value: "true")
                },
                status: SiteStatus.Online, isDeleted: null,
                Type: string.Empty),
            new(
                Id: "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
                Name: "Site 1",
                Address: "1 Park Row",
                PhoneNumber: "0113 1111111",
                OdsCode: "ABC01",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                location: new Location("Point", [.505, 65]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility>
                {
                    new Accessibility(Id: "accessibility/access_need_1", Value: "true")
                },
                status: SiteStatus.Online, isDeleted: null,
                Type: string.Empty)
        };

        var expectedSites = new List<SiteWithDistance>
        {
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC01",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [0.505, 65.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new Accessibility(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                Type: string.Empty),
                Distance: 234),
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bc",
                    Name: "Site 3",
                    Address: "3 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC03",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [.506, 65.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new Accessibility(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
                Distance: 281),
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC02",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [.507, 65.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new Accessibility(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
                Distance: 328)
        };

        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites);

        var result = await _sut.FindSitesByArea(new Coordinates { Longitude = 0.5, Latitude = 65 }, 50000, 50, []);
        result.Should().BeEquivalentTo(expectedSites);
    }

    [Fact]
    public async Task FindSitesByArea_ReturnsRequestedNumberOfSites()
    {
        var sites = new List<Site>
        {
            new(
                Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                Name: "Site 2",
                Address: "2 Park Row",
                PhoneNumber: "0113 1111111",
                OdsCode: "ABC02",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                location: new Location("Point", [.507, 65]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility>
                {
                    new Accessibility(Id: "accessibility/access_need_1", Value: "true")
                },
                status: SiteStatus.Online, isDeleted: null,
                Type: string.Empty),
            new(
                Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bc",
                Name: "Site 3",
                Address: "3 Park Row",
                PhoneNumber: "0113 1111111",
                OdsCode: "ABC03",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                location: new Location("Point", [.506, 65]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility>
                {
                    new Accessibility(Id: "accessibility/access_need_1", Value: "true")
                },
                status: SiteStatus.Online, isDeleted: null,
                Type: string.Empty),
            new(
                Id: "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
                Name: "Site 1",
                Address: "1 Park Row",
                PhoneNumber: "0113 1111111",
                OdsCode: "ABC01",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                location: new Location("Point", [.505, 65]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility>
                {
                    new Accessibility(Id: "accessibility/access_need_1", Value: "true")
                },
                status: SiteStatus.Online, isDeleted: null,
                Type: string.Empty)
        };

        var expectedSites = new List<SiteWithDistance>
        {
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC01",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [.505, 65.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new Accessibility(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status : SiteStatus.Online, isDeleted: null,
                Type: string.Empty),
                Distance: 234),
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bc",
                    Name: "Site 3",
                    Address: "3 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC03",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [.506, 65.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new Accessibility(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status : SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
                Distance: 281)
        };

        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites);

        var result = await _sut.FindSitesByArea(new Coordinates { Longitude = 0.5, Latitude = 65 }, 50000, 2, []);
        result.Should().BeEquivalentTo(expectedSites);
    }

    [Fact]
    public async Task FindSitesByArea_ReturnsSites_WithRequestedAccessNeeds()
    {
        var sites = new List<Site>
        {
            new(
                Id: "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
                Name: "Site 1",
                Address: "1 Park Row",
                PhoneNumber: "0113 1111111",
                OdsCode: "ABC01",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                location: new Location("Point", [.505, 50.0]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility> { new(Id: "accessibility/access_need_1", Value: "true") },
                status: SiteStatus.Online, isDeleted: null,
                Type: string.Empty),
            new(
                Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                Name: "Site 2",
                Address: "2 Park Row",
                PhoneNumber: "0113 1111111",
                OdsCode: "ABC02",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                location: new Location("Point", [.506, 50.0]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility> { new(Id: "accessibility/access_need_1", Value: "false") },
                status: SiteStatus.Online, isDeleted: null,
                Type: string.Empty),
        };

        var expectedSites = new List<SiteWithDistance>
        {
            new(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC01",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [0.505, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
                Distance: 357),
        };

        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites);

        var result = await _sut.FindSitesByArea(new Coordinates { Longitude = 0.5, Latitude = 50 }, 50000, 50,
            ["access_need_1"]);
        result.Should().BeEquivalentTo(expectedSites);
    }

    [Fact]
    public async Task FindSitesByArea_ReturnsNoSites_IfNoAccessNeedMatchesAreFound()
    {
        var sites = new List<Site>
        {
            new(
                Id: "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
                Name: "Site 1",
                Address: "1 Park Row",
                PhoneNumber: "0113 1111111",
                OdsCode: "ABC01",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                location: new Location("Point", [0.0, 50.0]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility>
                {
                    new(Id: "accessibility/accessibility/access_need_1", Value: "false")
                },
                status: SiteStatus.Online, isDeleted: null,
                Type: string.Empty),
            new(
                Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                Name: "Site 2",
                Address: "2 Park Row",
                PhoneNumber: "0113 1111111",
                OdsCode: "ABC02",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                location: new Location("Point", [.05, 50.0]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility>
                {
                    new(Id: "accessibility/accessibility/access_need_1", Value: "false")
                },
                status: SiteStatus.Online, isDeleted: null,
                Type: string.Empty),
        };

        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites);

        var result = await _sut.FindSitesByArea(new Coordinates { Longitude = 0.0, Latitude = 50 }, 50000, 50,
            ["access_need_1"]);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindSitesByArea_ReturnSitesBasedOnDistanceAndMaxRecords_IfNoAccessNeedsAreRequested()
    {
        var sites = new List<SiteWithDistance>
        {
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC01",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [0.04, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                Type: string.Empty),
                Distance: 2858),
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC02",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [0.05, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_2", Value: "true")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                Type: string.Empty),
                Distance: 3573),
        };

        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites.Select(s => s.Site));
        var result = await _sut.FindSitesByArea(new Coordinates { Longitude = 0.0, Latitude = 50 }, 50000, 50, []);
        result.Should().BeEquivalentTo(sites);
    }

    [Fact]
    public async Task FindSitesByArea_DoesNotReturnSitesWithNoAccessibilities_IfAccessNeedsAreRequested()
    {
        var sites = new List<Site>
        {
            new(
                Id: "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
                Name: "Site 1",
                Address: "1 Park Row",
                PhoneNumber: "0113 1111111",
                OdsCode: "ABC01",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                location: new Location("Point", [0.0, 50.0]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility>(),
                status: SiteStatus.Online, isDeleted: null,
                Type: string.Empty),
            new(
                Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                Name: "Site 2",
                Address: "2 Park Row",
                PhoneNumber: "0113 1111111",
                OdsCode: "ABC02",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                location: new Location("Point", [0.1, 50.1]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility> { new(Id: "accessibility/access_need_2", Value: "true") },
                status: SiteStatus.Online, isDeleted: null,
                Type: string.Empty)
        };
        var expectedSites = new List<SiteWithDistance>
        {
            new(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC02",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [0.1, 50.1]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_2", Value: "true")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
                Distance: 13213),
        };
        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites);
        var result = await _sut.FindSitesByArea(new Coordinates { Longitude = 0.0, Latitude = 50 }, 50000, 50,
            ["access_need_2"]);
        result.Should().BeEquivalentTo(expectedSites);
    }

    [Fact]
    public async Task FindSitesByArea_ReturnsSites_IfRequestedAccessNeedsAreEmpty()
    {
        var sites = new List<SiteWithDistance>
        {
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC01",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [0.04, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
                Distance: 2858),
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC02",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [0.05, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "false")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
                Distance: 3573),
        };
        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites.Select(s => s.Site));
        var result = await _sut.FindSitesByArea(new Coordinates { Longitude = 0.0, Latitude = 50 }, 50000, 50, [""]);
        result.Should().BeEquivalentTo(sites);
    }
    
    [Fact]
    public async Task FindSitesByArea_DoesntUseAvailabilityStore_WhenSiteSupportsServiceFilterNull()
    {
        var sites = new List<SiteWithDistance>
        {
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC01",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [0.04, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
                Distance: 2858),
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC02",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [0.05, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "false")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
                Distance: 3573)
        };
        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites.Select(s => s.Site));
        var result = await _sut.FindSitesByArea(new Coordinates { Longitude = 0.0, Latitude = 50 }, 50000, 50, [""]);
        result.Should().BeEquivalentTo(sites);
        _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()), Times.Never);
    }
    
    [Fact]
    public async Task FindSitesByArea_CallsAvailabilityStoreForEachSite_WhenSiteSupportsServiceFilterUsed_NoCache()
    {
        var sites = new List<SiteWithDistance>
        {
            new(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC01",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [0.04, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
                Distance: 2858),
            new(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC02",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [0.05, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "false")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
                Distance: 3573)
        };
        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites.Select(s => s.Site));
        _availabilityStore.Setup(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>())).ReturnsAsync(true);
        
        //set up a cache, but it's for a different date range, so its not used
        object outResult = new CacheService.LazySlideCacheObject(true, DateTimeOffset.UtcNow);
        _memoryCache.Setup(x => x.TryGetValue("LazySlide:site_6877d86e-c2df-4def-8508-e1eccf0ea6ba_supports_RSV:Adult_in_20251003_20251014", out outResult)).Returns(true);
        _memoryCache.Setup(x => x.TryGetValue("LazySlide:site_6877d86e-c2df-4def-8508-e1eccf0ea6bb_supports_RSV:Adult_in_20251003_20251014", out outResult)).Returns(true);

        var result = await _sut.FindSitesByArea(new Coordinates { Longitude = 0.0, Latitude = 50 }, 50000, 50, [""],
            false, new SiteSupportsServiceFilter(["RSV:Adult"], new DateOnly(2025, 10, 3), new DateOnly(2025, 10, 15)));
        result.Should().BeEquivalentTo(sites);

        var docIds = new List<string>() { "20251003", "20251004", "20251005","20251006","20251007","20251008","20251009","20251010", "20251011", "20251012","20251013","20251014","20251015"};
        
        _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync("6877d86e-c2df-4def-8508-e1eccf0ea6ba", new List<string> { "RSV:Adult" }, docIds), Times.Once);
        _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync("6877d86e-c2df-4def-8508-e1eccf0ea6bb", new List<string> { "RSV:Adult" }, docIds), Times.Once);
        
        //creates new correct cache for queried date range
        _memoryCache.Verify(x => x.CreateEntry("LazySlide:site_6877d86e-c2df-4def-8508-e1eccf0ea6ba_supports_RSV:Adult_in_20251003_20251015"), Times.Once);
        _memoryCache.Verify(x => x.CreateEntry("LazySlide:site_6877d86e-c2df-4def-8508-e1eccf0ea6bb_supports_RSV:Adult_in_20251003_20251015"), Times.Once);
        
        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString().Contains("GetSitesSupportingService returned 2 result(s) after 1 iteration(s) for services 'RSV:Adult'")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );
    }
    
    [Fact]
    public async Task FindSitesByArea_CallsAvailabilityStoreForEachSite_WhenSiteSupportsServiceFilterUsed_UsesCacheWhenPresent()
    {
        var sites = new List<SiteWithDistance>
        {
            new(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC01",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [0.04, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
                Distance: 2858),
            new(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC02",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [0.05, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "false")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
                Distance: 3573)
        };
        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites.Select(s => s.Site));
        
        object outResult = new CacheService.LazySlideCacheObject(true, DateTimeOffset.UtcNow);
        _memoryCache.Setup(x => x.TryGetValue("LazySlide:site_6877d86e-c2df-4def-8508-e1eccf0ea6ba_supports_RSV:Adult_in_20251003_20251015", out outResult)).Returns(true);
        _memoryCache.Setup(x => x.TryGetValue("LazySlide:site_6877d86e-c2df-4def-8508-e1eccf0ea6bb_supports_RSV:Adult_in_20251003_20251015", out outResult)).Returns(true);

        var result = await _sut.FindSitesByArea(new Coordinates { Longitude = 0.0, Latitude = 50 }, 50000, 50, [""],
            false, new SiteSupportsServiceFilter(["RSV:Adult"], new DateOnly(2025, 10, 3), new DateOnly(2025, 10, 15)));
        result.Should().BeEquivalentTo(sites);

        var docIds = new List<string>() { "20251003", "20251004", "20251005","20251006","20251007","20251008","20251009","20251010", "20251011", "20251012","20251013","20251014","20251015"};
        
        //doesn't call the store if cached
        _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync("6877d86e-c2df-4def-8508-e1eccf0ea6ba", new List<string> { "RSV:Adult" }, docIds), Times.Never);
        _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync("6877d86e-c2df-4def-8508-e1eccf0ea6bb", new List<string> { "RSV:Adult" }, docIds), Times.Never);
        
        _memoryCache.Verify(x => x.CreateEntry("LazySlide:site_6877d86e-c2df-4def-8508-e1eccf0ea6ba_supports_RSV:Adult_in_20251003_20251015"), Times.Never);
        _memoryCache.Verify(x => x.CreateEntry("LazySlide:site_6877d86e-c2df-4def-8508-e1eccf0ea6bb_supports_RSV:Adult_in_20251003_20251015"), Times.Never);
        
        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString().Contains("GetSitesSupportingService returned 2 result(s) after 1 iteration(s) for services 'RSV:Adult'")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );
    }
    
     [Fact]
    public async Task FindSitesByArea_CallsAvailabilityStoreForEachSiteBatched_WhenSiteSupportsServiceFilterUsed_PartialCached()
    {
        var invalidSites = new List<SiteWithDistance>();
        
        for (var i = 1; i < 21; i++)
        {
            var id = $"6877d86e-c2df-4def-8508-e1eccf0ea6{i:00}";
            invalidSites.Add(new SiteWithDistance(new Site(
                    Id: id,
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC02",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [0.05, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "false")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
                Distance: 3500+i));
            
            //invalid site results happen to not be cached
            object nullObject = null;
            _memoryCache.Setup(x => x.TryGetValue($"LazySlide:site_{id}_supports_RSV:Adult_in_20251003_20251006", out nullObject)).Returns(false);
        }

        var validSites = new List<SiteWithDistance>();
        
        for (double i = 1; i < 21; i++)
        {
            var longitude = 50.2d + (i / 100);
            var id = $"6877d86e-c2df-4def-8508-e1eccf0ea7{i:00}";
            validSites.Add(new SiteWithDistance(new Site(
                    Id: id,
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC02",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [0.05, longitude]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "false")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
                Distance: (int)(3700+i)));
            
            //valid site results happen to be cached
            object outResult = new CacheService.LazySlideCacheObject(true, DateTimeOffset.UtcNow);
            _memoryCache.Setup(x => x.TryGetValue($"LazySlide:site_{id}_supports_RSV:Adult_in_20251003_20251006", out outResult)).Returns(true);
        }
        
        var sites = invalidSites.Union(validSites).ToList();

        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites.Select(s => s.Site));

        //only setup for invalid sites
        for (var i = 1; i < 21; i++)
        {
            var id = $"{i:00}";
            _availabilityStore.Setup(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync($"6877d86e-c2df-4def-8508-e1eccf0ea6{id}", It.IsAny<List<string>>(), It.IsAny<List<string>>())).ReturnsAsync(false);
        }

        var result = await _sut.FindSitesByArea(new Coordinates { Longitude = 0.0, Latitude = 50 }, 50000, 1, [""],
            false, new SiteSupportsServiceFilter(["RSV:Adult"], new DateOnly(2025, 10, 3), new DateOnly(2025, 10, 06)));
        result.Single().Site.Id.Should().Be(validSites.First().Site.Id);

        var docIds = new List<string>() { "20251003", "20251004", "20251005", "20251006"};
        
        for (var i = 1; i < 21; i++)
        {
            var id = $"{i:00}";
            _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync($"6877d86e-c2df-4def-8508-e1eccf0ea6{id}", new List<string> { "RSV:Adult" }, docIds), Times.Once);
        }
        
        for (var i = 1; i < 21; i++)
        {
            //since the valid sites were cached, it shouldn't look up via DB
            var id = $"{i:00}";
            _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync($"6877d86e-c2df-4def-8508-e1eccf0ea7{id}", new List<string> { "RSV:Adult" }, docIds), Times.Never);
        }
        
        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString().Contains("GetSitesSupportingService returned 1 result(s) after 2 iteration(s) for services 'RSV:Adult'")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );
    }

    [Fact]
    public async Task FindSitesByArea_ReturnsEmptyCollection_WhenNoSitesAreFound()
    {
        var sites = Array.Empty<Site>();
        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites);
        var result = await _sut.FindSitesByArea(new Coordinates { Longitude = 0.5, Latitude = 65 }, 50000, 2,
            ["access_need_1"]);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindSitesByArea_ReadsFromCache_WhenPresent()
    {
        var sites = new List<SiteWithDistance>
        {
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC01",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [0.04, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
                Distance: 2858),
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC02",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [0.05, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "false")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
                Distance: 3573),
        };
        object outSites = sites.Select(s => s.Site);
        _memoryCache.Setup(x => x.TryGetValue("sites", out outSites)).Returns(true);
        var result = await _sut.FindSitesByArea(new Coordinates { Longitude = 0.0, Latitude = 50 }, 50000, 50, [""]);
        result.Should().BeEquivalentTo(sites);
        _siteStore.Verify(x => x.GetAllSites(), Times.Never());
    }

    [Fact]
    public async Task FindSitesByArea_IgnoresCache_WhenRequiredTo()
    {
        var sites = new List<SiteWithDistance>
        {
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC01",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [0.04, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
                Distance: 2858),
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC02",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [0.05, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "false")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
                Distance: 3573),
        };
        object outSites = sites.Take(1).Select(s => s.Site);
        _memoryCache.Setup(x => x.TryGetValue("sites", out outSites)).Returns(true);
        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites.Select(s => s.Site));

        var result =
            await _sut.FindSitesByArea(new Coordinates { Longitude = 0.0, Latitude = 50 }, 50000, 50, [""], true);
        result.Should().BeEquivalentTo(sites);
        _siteStore.Verify(x => x.GetAllSites(), Times.Once);
    }

    [Fact]
    public async Task FindSitesByArea_WritesToCache_AfterCacheMiss()
    {
        var sites = new List<SiteWithDistance>
        {
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC01",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [0.04, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
                Distance: 2858),
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC02",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [0.05, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "false")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
                Distance: 3573),
        };
        object outSites = null;
        _memoryCache.Setup(x => x.TryGetValue("sites", out outSites)).Returns(false);
        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites.Select(s => s.Site));

        var result = await _sut.FindSitesByArea(new Coordinates { Longitude = 0.0, Latitude = 50 }, 50000, 50, [""]);
        result.Should().BeEquivalentTo(sites);
        _siteStore.Verify(x => x.GetAllSites(), Times.Once);
        _memoryCache.Verify(x => x.CreateEntry("sites"), Times.Once);
    }

    [Fact]
    public async Task GetSiteByIdAsync_ReturnsRequestedSite()
    {
        const string siteId = "6877d86e-c2df-4def-8508-e1eccf0ea6ba";
        var site = new Site(
            Id: siteId,
            Name: "Site 1",
            Address: "1 Park Row",
            PhoneNumber: "0113 1111111",
            OdsCode: "ABC01",
            Region: "R1",
            IntegratedCareBoard: "ICB1",
            location: new Location("Point", [2.0, 70.0]),
            InformationForCitizens: "",
            Accessibilities: new List<Accessibility> { new Accessibility(Id: "Accessibility 1", Value: "true") },
            status: SiteStatus.Online, isDeleted: null,
            Type: string.Empty);

        var expectedSite = new Site(
            Id: siteId,
            Name: "Site 1",
            Address: "1 Park Row",
            PhoneNumber: "0113 1111111",
            OdsCode: "ABC01",
            Region: "R1",
            IntegratedCareBoard: "ICB1",
            location: new Location("Point", [2.0, 70.0]),
            InformationForCitizens: "",
            Accessibilities: new List<Accessibility> { new Accessibility(Id: "Accessibility 1", Value: "true") },
            status: SiteStatus.Online, isDeleted: null,
            Type: string.Empty);
        _siteStore.Setup(x => x.GetSiteById("6877d86e-c2df-4def-8508-e1eccf0ea6ba")).ReturnsAsync(site);

        var result = await _sut.GetSiteByIdAsync(siteId);
        result.Should().BeEquivalentTo(expectedSite);
    }

    [Theory]
    [InlineData("*")]
    [InlineData("site_details")]
    public async Task GetSiteByIdAsync_ReturnsDefault_WhenSiteIsNotFound(string scope)
    {
        const string siteId = "9a06bacd-e916-4c10-8263-21451ca751b8";
        _siteStore.Setup(x => x.GetSiteById(siteId)).ReturnsAsync((Site)null!);

        var result = await _sut.GetSiteByIdAsync(siteId, scope);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSitesPreview_CachedSitesFound_ReturningCachedSites()
    {
        var sites = new List<Site>()
        {
            new Site(
                    Id: "ABC01",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "odsCode1",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [0.04, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>() {new (Id: "accessibility/access_need_1", Value: "true")},
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
            new Site(
                    Id: "ABC02",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "odsCode2",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [0.05, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>() {new (Id: "accessibility/access_need_1", Value: "false")},
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty)
        };
        object outSites = sites;
        _memoryCache.Setup(x => x.TryGetValue("sites", out outSites)).Returns(true);
        var result = await _sut.GetSitesPreview();

        result.Count().Should().Be(2);
        _siteStore.Verify(x => x.GetAllSites(), Times.Never);
        _memoryCache.Verify(x => x.CreateEntry("sites"), Times.Never);
    }

    [Fact]
    public async Task GetSitesPreview_CachedSitesNotFound_ReturningAllSitesFromSiteStore()
    {
        var sites = new List<Site>()
        {
            new Site(
                    Id: "ABC01",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "odsCode1",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [0.04, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>() {new (Id: "accessibility/access_need_1", Value: "true")},
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
            new Site(
                    Id: "ABC02",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "odsCode2",
                    Region: "R1",
                    IntegratedCareBoard: "ICB2",
                    location: new Location("Point", [0.05, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>() {new (Id: "accessibility/access_need_1", Value: "false")},
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty)
        };
        object outSites = null;
        _memoryCache.Setup(x => x.TryGetValue("sites", out outSites)).Returns(true);
        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites);

        var result = await _sut.GetSitesPreview();

        result.Count().Should().Be(2);
        result.First().IntegratedCareBoard.Should().Be("ICB1");
        _siteStore.Verify(x => x.GetAllSites(), Times.Once);
        _memoryCache.Verify(x => x.CreateEntry("sites"), Times.Once);
    }

    [Fact]
    public async Task GetSitesInRegion_CachedSitesNotFound_ReturnsAllSitesFilteredWithMatchingRegion()
    {
        var sites = new List<Site>()
        {
            new(
                Id: "ABC01",
                Name: "Site 1",
                Address: "1 Park Row",
                PhoneNumber: "0113 1111111",
                OdsCode: "odsCode1",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                location: new Location("Point", [0.04, 50.0]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility>() {new (Id: "accessibility/access_need_1", Value: "true")},
                status: SiteStatus.Online, isDeleted: null,
                Type: string.Empty),
        };

        _siteStore.Setup(x => x.GetSitesInRegionAsync("R1")).ReturnsAsync(sites);

        var result = await _sut.GetSitesInRegion("R1");

        result.Count().Should().Be(1);
        result.First().Region.Should().Be("R1");

        _siteStore.Verify(x => x.GetSitesInRegionAsync("R1"), Times.Once);
    }

    [Fact]
    public async Task FindSitesByArea_FiltersOutOfflineSites()
    {
        var sites = new List<Site>
        {
            new(
                Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                Name: "Site 2",
                Address: "2 Park Row",
                PhoneNumber: "0113 1111111",
                OdsCode: "ABC02",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                location: new Location("Point", [.507, 65]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility>
                {
                    new(Id: "accessibility/access_need_1", Value: "true")
                },
                status: SiteStatus.Online, isDeleted: null,
                Type: string.Empty),
            new(
                Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bc",
                Name: "Site 3",
                Address: "3 Park Row",
                PhoneNumber: "0113 1111111",
                OdsCode: "ABC03",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                location: new Location("Point", [.506, 65]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility>
                {
                    new(Id: "accessibility/access_need_1", Value: "true")
                },
                status: SiteStatus.Online, isDeleted: null,
                Type: string.Empty),
            new(
                Id: "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
                Name: "Site 1",
                Address: "1 Park Row",
                PhoneNumber: "0113 1111111",
                OdsCode: "ABC01",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                location: new Location("Point", [.505, 65]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility>
                {
                    new(Id: "accessibility/access_need_1", Value: "true")
                },
                status: SiteStatus.Offline, isDeleted: null,
                Type: string.Empty)
        };

        var expectedSites = new List<SiteWithDistance>
        {
            new(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bc",
                    Name: "Site 3",
                    Address: "3 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC03",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [.506, 65.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
                Distance: 281),
            new(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC02",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    location: new Location("Point", [.507, 65.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status: SiteStatus.Online, isDeleted: null,
                    Type: string.Empty),
                Distance: 328)
        };
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.SiteStatus)).ReturnsAsync(true);

        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites);

        var result = await _sut.FindSitesByArea(new Coordinates { Longitude = 0.5, Latitude = 65 }, 50000, 50, []);
        result.Should().BeEquivalentTo(expectedSites);
    }

    [Fact]
    public async Task QuerySitesAsync_FiltersSitesOnAccessNeeds()
    {
        var filters = new List<SiteFilter>
        {
            new()
            {
                AccessNeeds = [ "test_access_need1", "test_access_need2" ],
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000
            }
        };
        var sites = new List<Site>
        {
            new("test123",
                "Test Site 1",
                string.Empty,
                string.Empty,
                "ODS1", "R1", "ICB1",
                string.Empty,
                new List<Accessibility>
                {
                    new("accessibility/test_access_need1", "true"),
                    new("accessibility/test_access_need2", "true")
                },
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                string.Empty),
            new("test321",
                "Test Site 2",
                string.Empty,
                string.Empty,
                "ODS2", "R2", "ICB2",
                string.Empty,
                new List<Accessibility>
                {
                    new("accessibility/test_access_need1", "true"),
                    new("accessibility/test_access_need2", "true")
                },
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                string.Empty),
            new("test456",
                "Test Site 3",
                string.Empty,
                string.Empty,
                "ODS3", "R3", "ICB3",
                string.Empty,
                new List<Accessibility>
                {
                    new("accessibility/test_access_need1", "false"),
                    new("accessibility/test_access_need2", "false")
                },
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                string.Empty),
        };

        _siteStore.Setup(x => x.GetAllSites())
            .ReturnsAsync(sites);

        var result = await _sut.QuerySitesAsync([.. filters], 50, true);

        result.Count().Should().Be(2);
        result.Any(r => r.Site.Id == "test456").Should().BeFalse();

        _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()), Times.Never);
    }

    [Fact]
    public async Task QuerySitesAsync_FiltersSitesOnDistance()
    {
        var filters = new List<SiteFilter>
        {
            new()
            {
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000
            }
        };
        var sites = new List<Site>
        {
            new("test123",
                "Test Site 1",
                string.Empty,
                string.Empty,
                "ODS1", "R1", "ICB1",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                string.Empty),
            new("test321",
                "Test Site 2",
                string.Empty,
                string.Empty,
                "ODS2", "R2", "ICB2",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                string.Empty),
            new("test456",
                "Test Site 3",
                string.Empty,
                string.Empty,
                "ODS3", "R3", "ICB3",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [1.6610648, 45.795467]),
                null,
                null,
                string.Empty),
        };

        _siteStore.Setup(x => x.GetAllSites())
            .ReturnsAsync(sites);

        var result = await _sut.QuerySitesAsync([.. filters], 50, true);

        result.Count().Should().Be(2);
        result.Any(r => r.Site.Id == "test456").Should().BeFalse();

        _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()), Times.Never);
    }

    [Fact]
    public async Task QuerySitesAsync_FiltersSitesOnService()
    {
        var filters = new List<SiteFilter>
        {
            new()
            {
                Availability = new()
                {
                    Services = ["RSV:Adult"],
                    From = new DateOnly(2025, 9, 1),
                    Until = new DateOnly(2025, 10, 1),
                },
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000
            }
        };
        var sites = new List<Site>
        {
            new("test123",
                "Test Site 1",
                string.Empty,
                string.Empty,
                "ODS1", "R1", "ICB1",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                string.Empty),
            new("test321",
                "Test Site 2",
                string.Empty,
                string.Empty,
                "ODS2", "R2", "ICB2",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                string.Empty),
            new("test456",
                "Test Site 3",
                string.Empty,
                string.Empty,
                "ODS3", "R3", "ICB3",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                string.Empty),
        };

        _siteStore.Setup(x => x.GetAllSites())
            .ReturnsAsync(sites);
        _availabilityStore.SetupSequence(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()))
            .ReturnsAsync(true)
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        var result = await _sut.QuerySitesAsync([.. filters], 50, true);

        result.Count().Should().Be(2);
        result.Any(r => r.Site.Id == "test456").Should().BeFalse();

        _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()), Times.Exactly(3));
    }

    [Fact]
    public async Task QuerySitesAsync_RemovesDuplicates()
    {
        var filters = new List<SiteFilter>
        {
            new()
            {
                Availability = new()
                {
                    Services = ["RSV:Adult"],
                    From = new DateOnly(2025, 9, 1),
                    Until = new DateOnly(2025, 10, 1),
                },
                AccessNeeds = [ "test_access_need1", "test_access_need2" ],
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000
            }
        };
        var sites = new List<Site>
        {
            new("test123",
                "Test Site 1",
                string.Empty,
                string.Empty,
                "ODS1", "R1", "ICB1",
                string.Empty,
                new List<Accessibility>
                {
                    new("accessibility/test_access_need1", "true"),
                    new("accessibility/test_access_need2", "true")
                },
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                string.Empty),
            new("test321",
                "Test Site 2",
                string.Empty,
                string.Empty,
                "ODS2", "R2", "ICB2",
                string.Empty,
                new List<Accessibility>
                {
                    new("accessibility/test_access_need1", "true"),
                    new("accessibility/test_access_need2", "true")
                },
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                string.Empty),
            new("test321",
                "Test Site 2",
                string.Empty,
                string.Empty,
                "ODS2", "R2", "ICB2",
                string.Empty,
                new List<Accessibility>
                {
                    new("accessibility/test_access_need1", "true"),
                    new("accessibility/test_access_need2", "true")
                },
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                string.Empty),
        };

        _siteStore.Setup(x => x.GetAllSites())
            .ReturnsAsync(sites);
        _availabilityStore.Setup(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()))
            .ReturnsAsync(true);

        var result = await _sut.QuerySitesAsync([.. filters], 50, true);

        result.Count().Should().Be(2);

        _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()), Times.Exactly(3));
    }

    [Fact]
    public async Task QuerySitesAsync_CorrectlyHandlesMultipleFilters_AndReturnsAllSites()
    {
        var filters = new List<SiteFilter>
        {
            new()
            {
                Availability = new()
                {
                    Services = ["RSV:Adult"],
                    From = new DateOnly(2025, 9, 1),
                    Until = new DateOnly(2025, 10, 1),
                },
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000
            },
            new() 
            {
                AccessNeeds = [ "test_access_need1", "test_access_need2" ],
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000
            }
        };
        var sites = new List<Site>
        {
            new("test123",
                "Test Site 1",
                string.Empty,
                string.Empty,
                "ODS1", "R1", "ICB1",
                string.Empty,
                new List<Accessibility>
                {
                    new("accessibility/test_access_need1", "true"),
                    new("accessibility/test_access_need2", "true")
                },
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                string.Empty),
            new("test321",
                "Test Site 2",
                string.Empty,
                string.Empty,
                "ODS2", "R2", "ICB2",
                string.Empty,
                new List<Accessibility>
                {
                    new("accessibility/test_access_need1", "true"),
                    new("accessibility/test_access_need2", "true")
                },
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                string.Empty),
            new("test456",
                "Test Site 3",
                string.Empty,
                string.Empty,
                "ODS3", "R3", "ICB3",
                string.Empty,
                new List<Accessibility>
                {
                    new("accessibility/test_access_need1", "false"),
                    new("accessibility/test_access_need2", "false")
                },
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                string.Empty),
        };

        _siteStore.Setup(x => x.GetAllSites())
            .ReturnsAsync(sites);
        _availabilityStore.Setup(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()))
            .ReturnsAsync(true);

        var result = await _sut.QuerySitesAsync([.. filters], 50, true);

        result.Count().Should().Be(3);

        _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()), Times.Exactly(3));
    }

    [Fact]
    public async Task QuerySitesAsync_CorrectlyHandlesMultipleFilters_AndFiltersOutInvalidSite()
    {
        var filters = new List<SiteFilter>
        {
            new()
            {
                Availability = new()
                {
                    Services = ["RSV:Adult"],
                    From = new DateOnly(2025, 9, 1),
                    Until = new DateOnly(2025, 10, 1),
                },
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000
            },
            new()
            {
                AccessNeeds = [ "test_access_need1", "test_access_need2" ],
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000
            }
        };
        var sites = new List<Site>
        {
            new("test123",
                "Test Site 1",
                string.Empty,
                string.Empty,
                "ODS1", "R1", "ICB1",
                string.Empty,
                new List<Accessibility>
                {
                    new("accessibility/test_access_need1", "true"),
                    new("accessibility/test_access_need2", "true")
                },
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                string.Empty),
            new("test321",
                "Test Site 2",
                string.Empty,
                string.Empty,
                "ODS2", "R2", "ICB2",
                string.Empty,
                new List<Accessibility>
                {
                    new("accessibility/test_access_need1", "true"),
                    new("accessibility/test_access_need2", "true")
                },
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                string.Empty),
            new("test456",
                "Test Site 3",
                string.Empty,
                string.Empty,
                "ODS3", "R3", "ICB3",
                string.Empty,
                new List<Accessibility>
                {
                    new("accessibility/test_access_need1", "false"),
                    new("accessibility/test_access_need2", "false")
                },
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                string.Empty),
            new("test654",
                "Test Site 4",
                string.Empty,
                string.Empty,
                "ODS4", "R4", "ICB4",
                string.Empty,
                new List<Accessibility>
                {
                    new("accessibility/test_access_need1", "false"),
                    new("accessibility/test_access_need2", "false")
                },
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                string.Empty),
        };

        _siteStore.Setup(x => x.GetAllSites())
            .ReturnsAsync(sites);
        _availabilityStore.SetupSequence(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()))
            .ReturnsAsync(true)
            .ReturnsAsync(true)
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        var result = await _sut.QuerySitesAsync([.. filters], 50, true);

        result.Count().Should().Be(3);

        _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()), Times.Exactly(4));
    }

    [Fact]
    public async Task QuerySitesAsync_FiltersBySiteType()
    {
        var filters = new List<SiteFilter>
        {
            new()
            {
                Types = [ "GP Practice" ],
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000
            }
        };
        var sites = new List<Site>
        {
            new("test123",
                "Test Site 1",
                string.Empty,
                string.Empty,
                "ODS1", "R1", "ICB1",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Pharmacy"),
            new("test321",
                "Test Site 2",
                string.Empty,
                string.Empty,
                "ODS2", "R2", "ICB2",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "gp PrActiCE"), // Ensure the filter is case insensitive
            new("test456",
                "Test Site 3",
                string.Empty,
                string.Empty,
                "ODS3", "R3", "ICB3",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "PCN Site"),
            new("test654",
                "Test Site 4",
                string.Empty,
                string.Empty,
                "ODS4", "R4", "ICB4",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Some other site type"),
        };

        _siteStore.Setup(x => x.GetAllSites())
            .ReturnsAsync(sites);

        var result = await _sut.QuerySitesAsync([.. filters], 50, true);

        result.Count().Should().Be(1);

        _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()), Times.Never);
    }

    [Fact]
    public async Task QuerySitesAsync_FiltersBySiteType_AndOdsCode()
    {
        var filters = new List<SiteFilter>
        {
            new()
            {
                Types = [ "GP Practice" ],
                OdsCode = "ODS2",
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000
            }
        };
        var sites = new List<Site>
        {
            new("test123",
                "Test Site 1",
                string.Empty,
                string.Empty,
                "ODS1", "R1", "ICB1",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Pharmacy"),
            new("test321",
                "Test Site 2",
                string.Empty,
                string.Empty,
                "ODS2", "R2", "ICB2",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "gp PrActiCE"), // Ensure the filter is case insensitive
            new("test456",
                "Test Site 3",
                string.Empty,
                string.Empty,
                "ODS3", "R3", "ICB3",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "PCN Site"),
            new("test654",
                "Test Site 4",
                string.Empty,
                string.Empty,
                "ODS4", "R4", "ICB4",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Some other site type"),
        };

        _siteStore.Setup(x => x.GetAllSites())
            .ReturnsAsync(sites);

        var result = await _sut.QuerySitesAsync([.. filters], 50, true);

        result.Count().Should().Be(1);

        _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()), Times.Never);
    }

    [Fact]
    public async Task QuerySitesAsync_FiltersBySiteType_AndOdsCode_AndDoesNotReturnAnySites()
    {
        var filters = new List<SiteFilter>
        {
            new()
            {
                Types = [ "GP Practice" ],
                OdsCode = "ODS3",
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000
            }
        };
        var sites = new List<Site>
        {
            new("test123",
                "Test Site 1",
                string.Empty,
                string.Empty,
                "ODS1", "R1", "ICB1",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Pharmacy"),
            new("test321",
                "Test Site 2",
                string.Empty,
                string.Empty,
                "ODS2", "R2", "ICB2",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "gp PrActiCE"), // Ensure the filter is case insensitive
            new("test456",
                "Test Site 3",
                string.Empty,
                string.Empty,
                "ODS3", "R3", "ICB3",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "PCN Site"),
            new("test654",
                "Test Site 4",
                string.Empty,
                string.Empty,
                "ODS4", "R4", "ICB4",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Some other site type"),
        };

        _siteStore.Setup(x => x.GetAllSites())
            .ReturnsAsync(sites);

        var result = await _sut.QuerySitesAsync([.. filters], 50, true);

        result.Count().Should().Be(0);

        _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()), Times.Never);
    }

    [Fact]
    public async Task QuerySitesAsync_FiltersByOdsCode()
    {
        var filters = new List<SiteFilter>
        {
            new()
            {
                OdsCode = "ODS2",
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000
            }
        };
        var sites = new List<Site>
        {
            new("test123",
                "Test Site 1",
                string.Empty,
                string.Empty,
                "ODS1", "R1", "ICB1",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Pharmacy"),
            new("test321",
                "Test Site 2",
                string.Empty,
                string.Empty,
                "ODS2", "R2", "ICB2",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "gp PrActiCE"), // Ensure the filter is case insensitive
            new("test456",
                "Test Site 3",
                string.Empty,
                string.Empty,
                "ODS3", "R3", "ICB3",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "PCN Site"),
            new("test654",
                "Test Site 4",
                string.Empty,
                string.Empty,
                "ODS4", "R4", "ICB4",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Some other site type"),
        };

        _siteStore.Setup(x => x.GetAllSites())
            .ReturnsAsync(sites);

        var result = await _sut.QuerySitesAsync([.. filters], 50, true);

        result.Count().Should().Be(1);

        _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()), Times.Never);
    }

    [Fact]
    public async Task QuerySitesAsync_FiltersSitesInPriorityOrder_AndStopsFilteringAfterMaxRecordsHit()
    {
        var filters = new List<SiteFilter>
        {
            new()
            {
                AccessNeeds = [ "test_access_need1", "test_access_need2" ],
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000,
                Priority = 2
            },
            new()
            {
                Types = ["GP Practice"],
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000,
                Priority = 1
            }
        };

        var sites = new List<Site>();
        for (var i = 0; i < 50; i++)
        {
            sites.Add(new(
                $"test{i}",
                $"Test Site {i}",
                string.Empty,
                string.Empty,
                $"ODS{i}", $"R{i}", $"ICB{i}",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "GP Practice"));
        }

        sites.Add(new(
            "test51",
            "Test Site 51",
            string.Empty,
            string.Empty,
            "ODS4", "R4", "ICB4",
            string.Empty,
            new List<Accessibility>
            {
                new("accessibility/test_access_need1", "false"),
                new("accessibility/test_access_need2", "false")
            },
            new Location("Point", [-1.6610648, 53.795467]),
            null,
            null,
            "Pharmacy"));

        _siteStore.Setup(x => x.GetAllSites())
            .ReturnsAsync(sites);

        var result = await _sut.QuerySitesAsync([.. filters], 50, true);

        result.Count().Should().Be(50);
        result.Any(x => x.Site.Id == "test51").Should().BeFalse();

        _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()), Times.Never);
    }

    [Fact]
    public async Task QuerySitesAsync_ShouldExcludeSiteTypesFromResults()
    {
        var filters = new List<SiteFilter>
        {
            new()
            {
                Types = [ "!GP Practice" ],
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000
            }
        };
        var sites = new List<Site>
        {
            new("test123",
                "Test Site 1",
                string.Empty,
                string.Empty,
                "ODS1", "R1", "ICB1",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Pharmacy"),
            new("test321",
                "Test Site 2",
                string.Empty,
                string.Empty,
                "ODS2", "R2", "ICB2",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "GP Practice"),
            new("test456",
                "Test Site 3",
                string.Empty,
                string.Empty,
                "ODS3", "R3", "ICB3",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "PCN Site"),
            new("test654",
                "Test Site 4",
                string.Empty,
                string.Empty,
                "ODS4", "R4", "ICB4",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Some other site type"),
        };

        _siteStore.Setup(x => x.GetAllSites())
            .ReturnsAsync(sites);

        var result = await _sut.QuerySitesAsync([.. filters], 50, true);

        result.Count().Should().Be(3);
        result.Any(r => r.Site.Id == "test321").Should().BeFalse();

        _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()), Times.Never);
    }

    [Fact]
    public async Task ToggleSiteSoftDeletionAsync_FailsToToggleSiteStatus()
    {
        _siteStore.Setup(x => x.ToggleSiteSoftDeletionAsync(It.IsAny<string>()))
            .ReturnsAsync(new OperationResult(false));

        var result = await _sut.ToggleSiteSoftDeletionAsync("TestSiteId");

        result.Success.Should().BeFalse();

        _memoryCache.Verify(x => x.Remove(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetSiteByIdAsync_ReturnsDefault_WhenSiteIsSoftDeleted()
    {
        const string siteId = "9a06bacd-e916-4c10-8263-21451ca751b8";
        var site = new Site(
            Id: siteId,
            Name: "Site 1",
            Address: "1 Park Row",
            PhoneNumber: "0113 1111111",
            OdsCode: "ABC01",
            Region: "R1",
            IntegratedCareBoard: "ICB1",
            location: new Location("Point", [2.0, 70.0]),
            InformationForCitizens: "",
            Accessibilities: new List<Accessibility> { new(Id: "Accessibility 1", Value: "true") },
            status: SiteStatus.Online, isDeleted: true,
            Type: string.Empty);
        _siteStore.Setup(x => x.GetSiteById(siteId)).ReturnsAsync(site);

        var result = await _sut.GetSiteByIdAsync(siteId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task QuerySitesAsync_HandlesNoSitesReturnedFromStore()
    {
        var filters = new List<SiteFilter>
        {
            new()
            {
                Types = [ "!GP Practice" ],
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000
            }
        };

        _siteStore.Setup(x => x.GetAllSites())
            .ReturnsAsync(new List<Site>());

        var result = await _sut.QuerySitesAsync([.. filters], 50, true);

        result.Count().Should().Be(0);
    }

    [Fact]
    public async Task QuerySitesAsync_FiltersInIndexOrder_WhenPrioritiesAreTheSame()
    {
        var filters = new List<SiteFilter>
        {
            new()
            {
                AccessNeeds = [ "test_access_need1", "test_access_need2" ],
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000,
                Priority = 1
            },
            new()
            {
                Types = ["GP Practice"],
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000,
                Priority = 1
            }
        };

        var sites = new List<Site>();
        for (var i = 0; i < 50; i++)
        {
            sites.Add(new(
                $"test{i}",
                $"Test Site {i}",
                string.Empty,
                string.Empty,
                $"ODS{i}", $"R{i}", $"ICB{i}",
                string.Empty,
                new List<Accessibility>
                {
                    new("accessibility/test_access_need1", "true"),
                    new("accessibility/test_access_need2", "true")
                },
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Pharmacy"));
        }

        sites.Add(new(
            "test51",
            "Test Site 51",
            string.Empty,
            string.Empty,
            "ODS4", "R4", "ICB4",
            string.Empty,
            new List<Accessibility>(),
            new Location("Point", [-1.6610648, 53.795467]),
            null,
            null,
            "GP Practice"));

        _siteStore.Setup(x => x.GetAllSites())
            .ReturnsAsync(sites);

        var result = await _sut.QuerySitesAsync([.. filters], 50, true);

        result.Count().Should().Be(50);
        result.Any(x => x.Site.Id == "test51").Should().BeFalse();

        _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()), Times.Never);
    }

    [Fact]
    public async Task QuerySitesAsync_FiltersOnPriority_WhenOnlyOneFilterHasPrioritySet()
    {
        var filters = new List<SiteFilter>
        {
            new()
            {
                AccessNeeds = [ "test_access_need1", "test_access_need2" ],
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000
            },
            new()
            {
                Types = ["GP Practice"],
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000,
                Priority = 1
            }
        };

        var sites = new List<Site>();
        for (var i = 0; i < 50; i++)
        {
            sites.Add(new(
                $"test{i}",
                $"Test Site {i}",
                string.Empty,
                string.Empty,
                $"ODS{i}", $"R{i}", $"ICB{i}",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "GP Practice"));
        }

        sites.Add(new(
            "test51",
            "Test Site 51",
            string.Empty,
            string.Empty,
            "ODS4", "R4", "ICB4",
            string.Empty,
            new List<Accessibility>
            {
                new("accessibility/test_access_need1", "false"),
                new("accessibility/test_access_need2", "false")
            },
            new Location("Point", [-1.6610648, 53.795467]),
            null,
            null,
            "Pharmacy"));

        _siteStore.Setup(x => x.GetAllSites())
            .ReturnsAsync(sites);

        var result = await _sut.QuerySitesAsync([.. filters], 50, true);

        result.Count().Should().Be(50);
        result.Any(x => x.Site.Id == "test51").Should().BeFalse();

        _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()), Times.Never);
    }

    [Fact]
    public async Task QuerySitesAsync_OnlyReturnsSiteOnce_WhenItMatchesMultipleFilters()
    {
        var filters = new List<SiteFilter>
        {
            new()
            {
                Types = [ "GP Practice" ],
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000
            },
            new()
            {
                AccessNeeds = [ "test_access_need1", "test_access_need2" ],
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000
            }
        };
        var sites = new List<Site>
        {
            new("test123",
                "Test Site 1",
                string.Empty,
                string.Empty,
                "ODS1", "R1", "ICB1",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Pharmacy"),
            new("test321",
                "Test Site 2",
                string.Empty,
                string.Empty,
                "ODS2", "R2", "ICB2",
                string.Empty,
                new List<Accessibility>
                {
                    new("accessibility/test_access_need1", "true"),
                    new("accessibility/test_access_need2", "true")
                },
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "GP Practice"),
            new("test456",
                "Test Site 3",
                string.Empty,
                string.Empty,
                "ODS3", "R3", "ICB3",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "PCN Site"),
            new("test654",
                "Test Site 4",
                string.Empty,
                string.Empty,
                "ODS4", "R4", "ICB4",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Some other site type"),
        };
        _siteStore.Setup(x => x.GetAllSites())
            .ReturnsAsync(sites);

        var result = await _sut.QuerySitesAsync([.. filters], 50, true);

        result.Count().Should().Be(1);
        result.First().Site.Id.Should().Be("test321");

        _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()), Times.Never);
    }

    [Fact]
    public async Task QuerySitesAsync_ReturnsSitesOnlyWhenAllServicesMatch()
    {
        var services = new List<string> { "COVID:5_11", "RSV:Adult" };
        var filters = new List<SiteFilter>
        {
            new()
            {
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000,
                Availability = new()
                {
                    Services = [ "RSV:Adult", "COVID:5_11", "COVID:5_11", "RSV:Adult" ],
                    From = new DateOnly(2025, 9, 1),
                    Until = new DateOnly(2025, 10, 1),
                }
            }
        };
        var sites = new List<Site>
        {
            new("test123",
                "Test Site 1",
                string.Empty,
                string.Empty,
                "ODS1", "R1", "ICB1",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Pharmacy"),
            new("test321",
                "Test Site 2",
                string.Empty,
                string.Empty,
                "ODS2", "R2", "ICB2",
                string.Empty,
                new List<Accessibility>
                {
                    new("accessibility/test_access_need1", "true"),
                    new("accessibility/test_access_need2", "true")
                },
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "GP Practice"),
            new("test456",
                "Test Site 3",
                string.Empty,
                string.Empty,
                "ODS3", "R3", "ICB3",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "PCN Site"),
            new("test654",
                "Test Site 4",
                string.Empty,
                string.Empty,
                "ODS4", "R4", "ICB4",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Some other site type"),
        };
        _siteStore.Setup(x => x.GetAllSites())
            .ReturnsAsync(sites);
        _availabilityStore.SetupSequence(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false)
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        var result = await _sut.QuerySitesAsync([.. filters], 50, true);

        result.Count().Should().Be(2);
        result.Any(x => x.Site.Id == "test654").Should().BeFalse();
        result.Any(x => x.Site.Id == "test321").Should().BeFalse();

        _availabilityStore.Verify(
            x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.Is<List<string>>(l => l.SequenceEqual(services)), It.IsAny<List<string>>()),
            Times.Exactly(4));
    }

    [Fact]
    public async Task QuerySitesAsync_CachingKeyUsedForFullServiceList()
    {
        var services = new List<string> { "RSV:Adult", "COVID:5_11" };
        var filters = new List<SiteFilter>
        {
            new()
            {
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000,
                Availability = new()
                {
                    Services = [.. services],
                    From = new DateOnly(2025, 9, 1),
                    Until = new DateOnly(2025, 10, 1),
                }
            }
        };
        var sites = new List<Site>
        {
            new("test123",
                "Test Site 1",
                string.Empty,
                string.Empty,
                "ODS1", "R1", "ICB1",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Pharmacy"),
            new("test321",
                "Test Site 2",
                string.Empty,
                string.Empty,
                "ODS2", "R2", "ICB2",
                string.Empty,
                new List<Accessibility>
                {
                    new("accessibility/test_access_need1", "true"),
                    new("accessibility/test_access_need2", "true")
                },
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "GP Practice"),
            new("test456",
                "Test Site 3",
                string.Empty,
                string.Empty,
                "ODS3", "R3", "ICB3",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "PCN Site"),
            new("test654",
                "Test Site 4",
                string.Empty,
                string.Empty,
                "ODS4", "R4", "ICB4",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Some other site type"),
        };
        _siteStore.Setup(x => x.GetAllSites())
            .ReturnsAsync(sites);
        
        object outResult = new CacheService.LazySlideCacheObject(true, DateTimeOffset.UtcNow);
        _memoryCache.Setup(x => x.TryGetValue(CacheService.LazySlideCacheKey("site_test123_supports_COVID:5_11_RSV:Adult_in_20250901_20251001"), out outResult)).Returns(true);
        _memoryCache.Setup(x => x.TryGetValue(CacheService.LazySlideCacheKey("site_test321_supports_COVID:5_11_RSV:Adult_in_20250901_20251001"), out outResult)).Returns(true);
        _memoryCache.Setup(x => x.TryGetValue(CacheService.LazySlideCacheKey("site_test456_supports_COVID:5_11_RSV:Adult_in_20250901_20251001"), out outResult)).Returns(true);
        _memoryCache.Setup(x => x.TryGetValue(CacheService.LazySlideCacheKey("site_test654_supports_COVID:5_11_RSV:Adult_in_20250901_20251001"), out outResult)).Returns(true);

        _ = await _sut.QuerySitesAsync([.. filters], 50, true);

        _memoryCache.Verify(x => x.CreateEntry(CacheService.LazySlideCacheKey("site_test123_supports_COVID:5_11_RSV:Adult_in_20250901_20251001")), Times.Never);
        _memoryCache.Verify(x => x.CreateEntry(CacheService.LazySlideCacheKey("site_test321_supports_COVID:5_11_RSV:Adult_in_20250901_20251001")), Times.Never);
        _memoryCache.Verify(x => x.CreateEntry(CacheService.LazySlideCacheKey("site_test456_supports_COVID:5_11_RSV:Adult_in_20250901_20251001")), Times.Never);
        _memoryCache.Verify(x => x.CreateEntry(CacheService.LazySlideCacheKey("site_test654_supports_COVID:5_11_RSV:Adult_in_20250901_20251001")), Times.Never);

        _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()), Times.Never);
    }

    [Fact]
    public async Task QuerySitesAsync_WritesToCache_WhenKeyNotPresentInitially()
    {
        var services = new List<string> { "RSV:Adult", "COVID:5_11" };
        var filters = new List<SiteFilter>
        {
            new()
            {
                Latitude = 53.796638,
                Longitude = -1.663038,
                SearchRadius = 1000,
                Availability = new()
                {
                    Services = [.. services],
                    From = new DateOnly(2025, 9, 1),
                    Until = new DateOnly(2025, 10, 1),
                }
            }
        };
        var sites = new List<Site>
        {
            new("test123",
                "Test Site 1",
                string.Empty,
                string.Empty,
                "ODS1", "R1", "ICB1",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Pharmacy"),
            new("test321",
                "Test Site 2",
                string.Empty,
                string.Empty,
                "ODS2", "R2", "ICB2",
                string.Empty,
                new List<Accessibility>
                {
                    new("accessibility/test_access_need1", "true"),
                    new("accessibility/test_access_need2", "true")
                },
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "GP Practice"),
            new("test456",
                "Test Site 3",
                string.Empty,
                string.Empty,
                "ODS3", "R3", "ICB3",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "PCN Site"),
            new("test654",
                "Test Site 4",
                string.Empty,
                string.Empty,
                "ODS4", "R4", "ICB4",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Some other site type"),
        };
        _siteStore.Setup(x => x.GetAllSites())
            .ReturnsAsync(sites);
        
        object outResult = true;
        _memoryCache.Setup(x => x.TryGetValue(CacheService.LazySlideCacheKey("site_test123_supports_COVID:5_11_RSV:Adult_in_20250901_20251001"), out outResult)).Returns(false);
        _memoryCache.Setup(x => x.TryGetValue(CacheService.LazySlideCacheKey("site_test321_supports_COVID:5_11_RSV:Adult_in_20250901_20251001"), out outResult)).Returns(false);
        _memoryCache.Setup(x => x.TryGetValue(CacheService.LazySlideCacheKey("site_test456_supports_COVID:5_11_RSV:Adult_in_20250901_20251001"), out outResult)).Returns(false);
        _memoryCache.Setup(x => x.TryGetValue(CacheService.LazySlideCacheKey("site_test654_supports_COVID:5_11_RSV:Adult_in_20250901_20251001"), out outResult)).Returns(false);

        _ = await _sut.QuerySitesAsync([.. filters], 50, true);

        _memoryCache.Verify(x => x.CreateEntry(CacheService.LazySlideCacheKey("site_test123_supports_COVID:5_11_RSV:Adult_in_20250901_20251001")), Times.Once);
        _memoryCache.Verify(x => x.CreateEntry(CacheService.LazySlideCacheKey("site_test321_supports_COVID:5_11_RSV:Adult_in_20250901_20251001")), Times.Once);
        _memoryCache.Verify(x => x.CreateEntry(CacheService.LazySlideCacheKey("site_test456_supports_COVID:5_11_RSV:Adult_in_20250901_20251001")), Times.Once);
        _memoryCache.Verify(x => x.CreateEntry(CacheService.LazySlideCacheKey("site_test654_supports_COVID:5_11_RSV:Adult_in_20250901_20251001")), Times.Once);

        _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()), Times.Exactly(4));
    }

    [Fact]
    public async Task FindSitesByArea_FiltersSitesOnMultipleServices()
    {
        var services = new List<string> { "COVID:5_11", "RSV:Adult" };
        var sites = new List<Site>
        {
            new("test123",
                "Test Site 1",
                string.Empty,
                string.Empty,
                "ODS1", "R1", "ICB1",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Pharmacy"),
            new("test321",
                "Test Site 2",
                string.Empty,
                string.Empty,
                "ODS2", "R2", "ICB2",
                string.Empty,
                new List<Accessibility>
                {
                    new("accessibility/test_access_need1", "true"),
                    new("accessibility/test_access_need2", "true")
                },
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "GP Practice"),
            new("test456",
                "Test Site 3",
                string.Empty,
                string.Empty,
                "ODS3", "R3", "ICB3",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "PCN Site"),
            new("test654",
                "Test Site 4",
                string.Empty,
                string.Empty,
                "ODS4", "R4", "ICB4",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Some other site type"),
        };
        _siteStore.Setup(x => x.GetAllSites())
            .ReturnsAsync(sites);
        _availabilityStore.SetupSequence(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false)
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        var result = await _sut.FindSitesByArea(
            new Coordinates
            { 
                Latitude = 53.796638,
                Longitude = -1.663038
            }, 
            1000,
            50,
            [],
            siteSupportsServiceFilter: new SiteSupportsServiceFilter(
                ["RSV:Adult", "COVID:5_11", "RSV:Adult", "COVID:5_11"],
                new DateOnly(2025, 9, 1),
                new DateOnly(2025, 10, 1)
                )
            );

        result.Count().Should().Be(2);
        result.Any(x => x.Site.Id == "test654").Should().BeFalse();
        result.Any(x => x.Site.Id == "test321").Should().BeFalse();

        _availabilityStore.Verify(
            x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.Is<List<string>>(l => l.SequenceEqual(services)), It.IsAny<List<string>>()),
            Times.Exactly(4));
    }

    [Fact]
    public async Task FindSitesByArea_CachingKeyUsedForFullServiceList()
    {
        var services = new List<string> { "RSV:Adult", "COVID:5_11" };
        var sites = new List<Site>
        {
            new("test123",
                "Test Site 1",
                string.Empty,
                string.Empty,
                "ODS1", "R1", "ICB1",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Pharmacy"),
            new("test321",
                "Test Site 2",
                string.Empty,
                string.Empty,
                "ODS2", "R2", "ICB2",
                string.Empty,
                new List<Accessibility>
                {
                    new("accessibility/test_access_need1", "true"),
                    new("accessibility/test_access_need2", "true")
                },
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "GP Practice"),
            new("test456",
                "Test Site 3",
                string.Empty,
                string.Empty,
                "ODS3", "R3", "ICB3",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "PCN Site"),
            new("test654",
                "Test Site 4",
                string.Empty,
                string.Empty,
                "ODS4", "R4", "ICB4",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Some other site type"),
        };
        _siteStore.Setup(x => x.GetAllSites())
            .ReturnsAsync(sites);
        _availabilityStore.SetupSequence(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false)
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        object outResult = new CacheService.LazySlideCacheObject(true, DateTimeOffset.UtcNow);
        _memoryCache.Setup(x => x.TryGetValue(CacheService.LazySlideCacheKey("site_test123_supports_COVID:5_11_RSV:Adult_in_20250901_20251001"), out outResult)).Returns(true);
        _memoryCache.Setup(x => x.TryGetValue(CacheService.LazySlideCacheKey("site_test321_supports_COVID:5_11_RSV:Adult_in_20250901_20251001"), out outResult)).Returns(true);
        _memoryCache.Setup(x => x.TryGetValue(CacheService.LazySlideCacheKey("site_test456_supports_COVID:5_11_RSV:Adult_in_20250901_20251001"), out outResult)).Returns(true);
        _memoryCache.Setup(x => x.TryGetValue(CacheService.LazySlideCacheKey("site_test654_supports_COVID:5_11_RSV:Adult_in_20250901_20251001"), out outResult)).Returns(true);

        _ = await _sut.FindSitesByArea(
            new Coordinates
            {
                Latitude = 53.796638,
                Longitude = -1.663038
            },
            1000,
            50,
            [],
            siteSupportsServiceFilter: new SiteSupportsServiceFilter(
                ["RSV:Adult", "COVID:5_11"],
                new DateOnly(2025, 9, 1),
                new DateOnly(2025, 10, 1)
                )
            );

        _memoryCache.Verify(x => x.CreateEntry(CacheService.LazySlideCacheKey("site_test123_supports_COVID:5_11_RSV:Adult_in_20250901_20251001")), Times.Never);
        _memoryCache.Verify(x => x.CreateEntry(CacheService.LazySlideCacheKey("site_test321_supports_COVID:5_11_RSV:Adult_in_20250901_20251001")), Times.Never);
        _memoryCache.Verify(x => x.CreateEntry(CacheService.LazySlideCacheKey("site_test456_supports_COVID:5_11_RSV:Adult_in_20250901_20251001")), Times.Never);
        _memoryCache.Verify(x => x.CreateEntry(CacheService.LazySlideCacheKey("site_test654_supports_COVID:5_11_RSV:Adult_in_20250901_20251001")), Times.Never);

        _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()), Times.Never);
    }

    [Fact]
    public async Task FindSitesByArea_WritesToCache_WhenKeyNotPresentInitially()
    {
        var services = new List<string> { "RSV:Adult", "COVID:5_11" };
        var sites = new List<Site>
        {
            new("test123",
                "Test Site 1",
                string.Empty,
                string.Empty,
                "ODS1", "R1", "ICB1",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Pharmacy"),
            new("test321",
                "Test Site 2",
                string.Empty,
                string.Empty,
                "ODS2", "R2", "ICB2",
                string.Empty,
                new List<Accessibility>
                {
                    new("accessibility/test_access_need1", "true"),
                    new("accessibility/test_access_need2", "true")
                },
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "GP Practice"),
            new("test456",
                "Test Site 3",
                string.Empty,
                string.Empty,
                "ODS3", "R3", "ICB3",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "PCN Site"),
            new("test654",
                "Test Site 4",
                string.Empty,
                string.Empty,
                "ODS4", "R4", "ICB4",
                string.Empty,
                new List<Accessibility>(),
                new Location("Point", [-1.6610648, 53.795467]),
                null,
                null,
                "Some other site type"),
        };
        _siteStore.Setup(x => x.GetAllSites())
            .ReturnsAsync(sites);
        _availabilityStore.SetupSequence(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()))
            .ReturnsAsync(true)
            .ReturnsAsync(false)
            .ReturnsAsync(true)
            .ReturnsAsync(false);

        object outResult = true;
        _memoryCache.Setup(x => x.TryGetValue(CacheService.LazySlideCacheKey("site_test123_supports_COVID:5_11_RSV:Adult_in_20250901_20251001"), out outResult)).Returns(false);
        _memoryCache.Setup(x => x.TryGetValue(CacheService.LazySlideCacheKey("site_test321_supports_COVID:5_11_RSV:Adult_in_20250901_20251001"), out outResult)).Returns(false);
        _memoryCache.Setup(x => x.TryGetValue(CacheService.LazySlideCacheKey("site_test456_supports_COVID:5_11_RSV:Adult_in_20250901_20251001"), out outResult)).Returns(false);
        _memoryCache.Setup(x => x.TryGetValue(CacheService.LazySlideCacheKey("site_test654_supports_COVID:5_11_RSV:Adult_in_20250901_20251001"), out outResult)).Returns(false);

        _ = await _sut.FindSitesByArea(
            new Coordinates
            {
                Latitude = 53.796638,
                Longitude = -1.663038
            },
            1000,
            50,
            [],
            siteSupportsServiceFilter: new SiteSupportsServiceFilter(
                ["RSV:Adult", "COVID:5_11"],
                new DateOnly(2025, 9, 1),
                new DateOnly(2025, 10, 1)
                )
            );

        _memoryCache.Verify(x => x.CreateEntry(CacheService.LazySlideCacheKey("site_test123_supports_COVID:5_11_RSV:Adult_in_20250901_20251001")), Times.Once);
        _memoryCache.Verify(x => x.CreateEntry(CacheService.LazySlideCacheKey("site_test321_supports_COVID:5_11_RSV:Adult_in_20250901_20251001")), Times.Once);
        _memoryCache.Verify(x => x.CreateEntry(CacheService.LazySlideCacheKey("site_test456_supports_COVID:5_11_RSV:Adult_in_20250901_20251001")), Times.Once);
        _memoryCache.Verify(x => x.CreateEntry(CacheService.LazySlideCacheKey("site_test654_supports_COVID:5_11_RSV:Adult_in_20250901_20251001")), Times.Once);

        _availabilityStore.Verify(x => x.SiteSupportsAllServicesOnSingleDateInRangeAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()), Times.Exactly(4));
    }
}
