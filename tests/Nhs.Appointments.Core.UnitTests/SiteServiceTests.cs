using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Nhs.Appointments.Core.UnitTests;

public class SiteServiceTests
{
    private readonly Mock<ICacheEntry> _cacheEntry = new();
    private readonly Mock<IMemoryCache> _memoryCache = new();
    private readonly Mock<ISiteStore> _siteStore = new();
    private readonly Mock<IAvailabilityStore> _availabilityStore = new();
    private readonly Mock<ILogger<ISiteService>> _logger = new();
    private readonly SiteService _sut;

    public SiteServiceTests()
    {
        _sut = new SiteService(_siteStore.Object, _availabilityStore.Object, _memoryCache.Object, _logger.Object, TimeProvider.System, _featureToggleHelper.Object);
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
                Location: new Location(Type: "Point", Coordinates: [.507, 65]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility>
                {
                    new Accessibility(Id: "accessibility/access_need_1", Value: "true")
                },
                status: SiteStatus.Online),
            new(
                Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bc",
                Name: "Site 3",
                Address: "3 Park Row",
                PhoneNumber: "0113 1111111",
                OdsCode: "ABC03",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                Location: new Location(Type: "Point", Coordinates: [.506, 65]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility>
                {
                    new Accessibility(Id: "accessibility/access_need_1", Value: "true")
                },
                status: SiteStatus.Online),
            new(
                Id: "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
                Name: "Site 1",
                Address: "1 Park Row",
                PhoneNumber: "0113 1111111",
                OdsCode: "ABC01",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                Location: new Location(Type: "Point", Coordinates: [.505, 65]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility>
                {
                    new Accessibility(Id: "accessibility/access_need_1", Value: "true")
                },
                status: SiteStatus.Online)
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
                    Location: new Location(Type: "Point", Coordinates: [0.505, 65.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new Accessibility(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status: SiteStatus.Online),
                Distance: 234),
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bc",
                    Name: "Site 3",
                    Address: "3 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC03",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [.506, 65.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new Accessibility(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status: SiteStatus.Online),
                Distance: 281),
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC02",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [.507, 65.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new Accessibility(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status: SiteStatus.Online),
                Distance: 328)
        };

        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites);

        var result = await _sut.FindSitesByArea(0.5, 65, 50000, 50, []);
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
                Location: new Location(Type: "Point", Coordinates: [.507, 65]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility>
                {
                    new Accessibility(Id: "accessibility/access_need_1", Value: "true")
                },
                status: SiteStatus.Online),
            new(
                Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bc",
                Name: "Site 3",
                Address: "3 Park Row",
                PhoneNumber: "0113 1111111",
                OdsCode: "ABC03",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                Location: new Location(Type: "Point", Coordinates: [.506, 65]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility>
                {
                    new Accessibility(Id: "accessibility/access_need_1", Value: "true")
                },
                status: SiteStatus.Online),
            new(
                Id: "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
                Name: "Site 1",
                Address: "1 Park Row",
                PhoneNumber: "0113 1111111",
                OdsCode: "ABC01",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                Location: new Location(Type: "Point", Coordinates: [.505, 65]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility>
                {
                    new Accessibility(Id: "accessibility/access_need_1", Value: "true")
                },
                status: SiteStatus.Online)
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
                    Location: new Location(Type: "Point", Coordinates: [.505, 65.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new Accessibility(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status : SiteStatus.Online),
                Distance: 234),
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bc",
                    Name: "Site 3",
                    Address: "3 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC03",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [.506, 65.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new Accessibility(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status : SiteStatus.Online),
                Distance: 281)
        };

        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites);

        var result = await _sut.FindSitesByArea(0.5, 65, 50000, 2, []);
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
                Location: new Location(Type: "Point", Coordinates: [.505, 50.0]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility> { new(Id: "accessibility/access_need_1", Value: "true") },
                status: SiteStatus.Online),
            new(
                Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                Name: "Site 2",
                Address: "2 Park Row",
                PhoneNumber: "0113 1111111",
                OdsCode: "ABC02",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                Location: new Location(Type: "Point", Coordinates: [.506, 50.0]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility> { new(Id: "accessibility/access_need_1", Value: "false") },
                status: SiteStatus.Online),
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
                    Location: new Location(Type: "Point", Coordinates: [0.505, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status: SiteStatus.Online),
                Distance: 357),
        };

        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites);

        var result = await _sut.FindSitesByArea(0.5, 50, 50000, 50, ["access_need_1"]);
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
                Location: new Location(Type: "Point", Coordinates: [0.0, 50.0]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility>
                {
                    new(Id: "accessibility/accessibility/access_need_1", Value: "false")
                },
                status: SiteStatus.Online),
            new(
                Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                Name: "Site 2",
                Address: "2 Park Row",
                PhoneNumber: "0113 1111111",
                OdsCode: "ABC02",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                Location: new Location(Type: "Point", Coordinates: [.05, 50.0]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility>
                {
                    new(Id: "accessibility/accessibility/access_need_1", Value: "false")
                },
                status: SiteStatus.Online),
        };

        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites);

        var result = await _sut.FindSitesByArea(0.0, 50, 50000, 50, ["access_need_1"]);
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
                    Location: new Location(Type: "Point", Coordinates: [0.04, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status: SiteStatus.Online),
                Distance: 2858),
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC02",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.05, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_2", Value: "true")
                    },
                    status: SiteStatus.Online),
                Distance: 3573),
        };

        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites.Select(s => s.Site));
        var result = await _sut.FindSitesByArea(0.0, 50, 50000, 50, []);
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
                Location: new Location(Type: "Point", Coordinates: [0.0, 50.0]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility>(),
                status: SiteStatus.Online),
            new(
                Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                Name: "Site 2",
                Address: "2 Park Row",
                PhoneNumber: "0113 1111111",
                OdsCode: "ABC02",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                Location: new Location(Type: "Point", Coordinates: [0.1, 50.1]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility> { new(Id: "accessibility/access_need_2", Value: "true") },
                status: SiteStatus.Online)
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
                    Location: new Location(Type: "Point", Coordinates: [0.1, 50.1]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_2", Value: "true")
                    },
                    status: SiteStatus.Online),
                Distance: 13213),
        };
        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites);
        var result = await _sut.FindSitesByArea(0.0, 50, 50000, 50, ["access_need_2"]);
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
                    Location: new Location(Type: "Point", Coordinates: [0.04, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status: SiteStatus.Online),
                Distance: 2858),
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC02",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.05, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "false")
                    },
                    status: SiteStatus.Online),
                Distance: 3573),
        };
        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites.Select(s => s.Site));
        var result = await _sut.FindSitesByArea(0.0, 50, 50000, 50, [""]);
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
                    Location: new Location(Type: "Point", Coordinates: [0.04, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "true")
                    }),
                Distance: 2858),
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC02",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.05, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "false")
                    }),
                Distance: 3573)
        };
        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites.Select(s => s.Site));
        var result = await _sut.FindSitesByArea(0.0, 50, 50000, 50, [""]);
        result.Should().BeEquivalentTo(sites);
        _availabilityStore.Verify(x => x.SiteOffersServiceDuringPeriod(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()), Times.Never);
    }
    
    [Fact]
    public async Task FindSitesByArea_CallsAvailabilityStoreForEachSite_WhenSiteSupportsServiceFilterUsed()
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
                    Location: new Location(Type: "Point", Coordinates: [0.04, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "true")
                    }),
                Distance: 2858),
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC02",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.05, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "false")
                    }),
                Distance: 3573)
        };
        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites.Select(s => s.Site));
        _availabilityStore.Setup(x => x.SiteOffersServiceDuringPeriod(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(true);
        var result = await _sut.FindSitesByArea(0.0, 50, 50000, 50, [""], false, new SiteSupportsServiceFilter("RSV:Adult", new DateOnly(2025,10,3), new DateOnly(2025,10,15)));
        result.Should().BeEquivalentTo(sites);

        var docIds = new List<string>() { "20251003", "20251004", "20251005","20251006","20251007","20251008","20251009","20251010", "20251011", "20251012","20251013","20251014","20251015"};
        
        _availabilityStore.Verify(x => x.SiteOffersServiceDuringPeriod("6877d86e-c2df-4def-8508-e1eccf0ea6ba", "RSV:Adult", docIds), Times.Once);
        _availabilityStore.Verify(x => x.SiteOffersServiceDuringPeriod("6877d86e-c2df-4def-8508-e1eccf0ea6bb", "RSV:Adult", docIds), Times.Once);
        
        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString().Contains("GetSitesSupportingService returned 2 result(s) after 1 iteration(s) for service 'RSV:Adult'")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );
    }
    
     [Fact]
    public async Task FindSitesByArea_CallsAvailabilityStoreForEachSiteBatched_WhenSiteSupportsServiceFilterUsed()
    {
        var invalidSites = new List<SiteWithDistance>
        {
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC01",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.04, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "true")
                    }),
                Distance: 2858),
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC02",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.05, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "false")
                    }),
                Distance: 3573)
        };
        var validSites = new List<SiteWithDistance>
        {
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bc",
                    Name: "Site 3",
                    Address: "3 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC03",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.05, 50.2]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "false")
                    }),
                Distance: 22522),
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bd",
                    Name: "Site 4",
                    Address: "4 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC04",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.05, 50.3]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "false")
                    }),
                Distance: 33546)
        };
        
        var sites = invalidSites.Union(validSites).ToList();
        
        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites.Select(s => s.Site));
        
        _availabilityStore.Setup(x => x.SiteOffersServiceDuringPeriod("6877d86e-c2df-4def-8508-e1eccf0ea6ba", It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(false);
        _availabilityStore.Setup(x => x.SiteOffersServiceDuringPeriod("6877d86e-c2df-4def-8508-e1eccf0ea6bb", It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(false);
        _availabilityStore.Setup(x => x.SiteOffersServiceDuringPeriod("6877d86e-c2df-4def-8508-e1eccf0ea6bc", It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(true);
        _availabilityStore.Setup(x => x.SiteOffersServiceDuringPeriod("6877d86e-c2df-4def-8508-e1eccf0ea6bd", It.IsAny<string>(), It.IsAny<List<string>>())).ReturnsAsync(true);
        
        var result = await _sut.FindSitesByArea(0.0, 50, 50000, 1, [""], false, new SiteSupportsServiceFilter("RSV:Adult", new DateOnly(2025,10,3), new DateOnly(2025,10,06)));
        result.Should().BeEquivalentTo([validSites.First()]);

        var docIds = new List<string>() { "20251003", "20251004", "20251005", "20251006"};
        
        _availabilityStore.Verify(x => x.SiteOffersServiceDuringPeriod("6877d86e-c2df-4def-8508-e1eccf0ea6ba", "RSV:Adult", docIds), Times.Once);
        _availabilityStore.Verify(x => x.SiteOffersServiceDuringPeriod("6877d86e-c2df-4def-8508-e1eccf0ea6bb", "RSV:Adult", docIds), Times.Once);
        _availabilityStore.Verify(x => x.SiteOffersServiceDuringPeriod("6877d86e-c2df-4def-8508-e1eccf0ea6bc", "RSV:Adult", docIds), Times.Once);
        _availabilityStore.Verify(x => x.SiteOffersServiceDuringPeriod("6877d86e-c2df-4def-8508-e1eccf0ea6bd", "RSV:Adult", docIds), Times.Once);
        
        _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString().Contains("GetSitesSupportingService returned 1 result(s) after 2 iteration(s) for service 'RSV:Adult'")
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
        var result = await _sut.FindSitesByArea(0.5, 65, 50000, 2, ["access_need_1"]);
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
                    Location: new Location(Type: "Point", Coordinates: [0.04, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status: SiteStatus.Online),
                Distance: 2858),
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC02",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.05, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "false")
                    },
                    status: SiteStatus.Online),
                Distance: 3573),
        };
        object outSites = sites.Select(s => s.Site);
        _memoryCache.Setup(x => x.TryGetValue("sites", out outSites)).Returns(true);
        var result = await _sut.FindSitesByArea(0.0, 50, 50000, 50, [""]);
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
                    Location: new Location(Type: "Point", Coordinates: [0.04, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status: SiteStatus.Online),
                Distance: 2858),
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC02",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.05, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "false")
                    },
                    status: SiteStatus.Online),
                Distance: 3573),
        };
        object outSites = sites.Take(1).Select(s => s.Site);
        _memoryCache.Setup(x => x.TryGetValue("sites", out outSites)).Returns(true);
        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites.Select(s => s.Site));

        var result = await _sut.FindSitesByArea(0.0, 50, 50000, 50, [""], true);
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
                    Location: new Location(Type: "Point", Coordinates: [0.04, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "true")
                    },
                    status: SiteStatus.Online),
                Distance: 2858),
            new SiteWithDistance(new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6bb",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "ABC02",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.05, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>
                    {
                        new(Id: "accessibility/access_need_1", Value: "false")
                    },
                    status: SiteStatus.Online),
                Distance: 3573),
        };
        object outSites = null;
        _memoryCache.Setup(x => x.TryGetValue("sites", out outSites)).Returns(false);
        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites.Select(s => s.Site));

        var result = await _sut.FindSitesByArea(0.0, 50, 50000, 50, [""], true);
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
            Location: new Location(Type: "Point", Coordinates: [2.0, 70.0]),
            InformationForCitizens: "",
            Accessibilities: new List<Accessibility> { new Accessibility(Id: "Accessibility 1", Value: "true") },
            status: SiteStatus.Online);

        var expectedSite = new Site(
            Id: siteId,
            Name: "Site 1",
            Address: "1 Park Row",
            PhoneNumber: "0113 1111111",
            OdsCode: "ABC01",
            Region: "R1",
            IntegratedCareBoard: "ICB1",
            Location: new Location(Type: "Point", Coordinates: [2.0, 70.0]),
            InformationForCitizens: "",
            Accessibilities: new List<Accessibility> { new Accessibility(Id: "Accessibility 1", Value: "true") },
            status: SiteStatus.Online);
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
                    Location: new Location(Type: "Point", Coordinates: [0.04, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>() {new (Id: "accessibility/access_need_1", Value: "true")},
                    status: SiteStatus.Online),
            new Site(
                    Id: "ABC02",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "odsCode2",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.05, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>() {new (Id: "accessibility/access_need_1", Value: "false")},
                    status: SiteStatus.Online)
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
                    Location: new Location(Type: "Point", Coordinates: [0.04, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>() {new (Id: "accessibility/access_need_1", Value: "true")},
                    status: SiteStatus.Online),
            new Site(
                    Id: "ABC02",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "odsCode2",
                    Region: "R1",
                    IntegratedCareBoard: "ICB2",
                    Location: new Location(Type: "Point", Coordinates: [0.05, 50.0]),
                    InformationForCitizens: "",
                    Accessibilities: new List<Accessibility>() {new (Id: "accessibility/access_need_1", Value: "false")},
                    status: SiteStatus.Online)
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
                Location: new Location(Type: "Point", Coordinates: [0.04, 50.0]),
                InformationForCitizens: "",
                Accessibilities: new List<Accessibility>() {new (Id: "accessibility/access_need_1", Value: "true")},
                status: SiteStatus.Online),
        };

        _siteStore.Setup(x => x.GetSitesInRegionAsync("R1")).ReturnsAsync(sites);

        var result = await _sut.GetSitesInRegion("R1");

        result.Count().Should().Be(1);
        result.First().Region.Should().Be("R1");

        _siteStore.Verify(x => x.GetSitesInRegionAsync("R1"), Times.Once);
    }
}
