using Microsoft.Extensions.Caching.Memory;

namespace Nhs.Appointments.Core.UnitTests;
public class SiteServiceTests
{
    private readonly SiteService _sut;
    private readonly Mock<ISiteStore> _siteStore = new();
    private readonly Mock<IMemoryCache> _memoryCache = new();    
    private readonly Mock<ICacheEntry> _cacheEntry = new();

    public SiteServiceTests()
    {
        _sut = new SiteService(_siteStore.Object, _memoryCache.Object, TimeProvider.System);
        _memoryCache.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(_cacheEntry.Object);
    }

    [Fact]
    public async Task FindSitesByArea_ReturnsSitesOrderedByDistance_InAscendingOrder()
    {
        var sites = new List<Site>()
        {
            new (
                    Id: "ABC02",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [.507, 65]),
                    AttributeValues: new List<AttributeValue>() {new AttributeValue(Id: "accessibility/access_need_1", Value: "true")}),                
            new (
                    Id: "ABC03",
                    Name: "Site 3",
                    Address: "3 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [.506, 65]),
                AttributeValues: new List<AttributeValue>() {new AttributeValue(Id: "accessibility/access_need_1", Value: "true")}),                
            new (
                    Id: "ABC01",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [.505, 65]),
                AttributeValues: new List<AttributeValue>() {new AttributeValue(Id: "accessibility/access_need_1", Value: "true")})                
        };

        var expectedSites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(
                    Id: "ABC01",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.505, 65.0]),
                AttributeValues: new List<AttributeValue>() {new AttributeValue(Id: "accessibility/access_need_1", Value: "true")}),
                Distance: 234),
            new SiteWithDistance(new Site(
                    Id: "ABC03",
                    Name: "Site 3",
                    Address: "3 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [.506, 65.0]),
                AttributeValues: new List<AttributeValue>() {new AttributeValue(Id: "accessibility/access_need_1", Value: "true")}),
                Distance: 281),
            new SiteWithDistance(new Site(
                    Id: "ABC02",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [.507, 65.0]),
                    AttributeValues: new List<AttributeValue>() {new AttributeValue(Id: "accessibility/access_need_1", Value: "true")}),
                Distance: 328)
        };
        
        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites);

        var result = await _sut.FindSitesByArea(0.5, 65, 50000, 50, []);
        result.Should().BeEquivalentTo(expectedSites);
    }

    [Fact]
    public async Task FindSitesByArea_ReturnsRequestedNumberOfSites()
    {
        var sites = new List<Site>()
        {
            new (
                    Id: "ABC02",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [.507, 65]),
                    AttributeValues: new List<AttributeValue>() {new AttributeValue(Id: "accessibility/access_need_1", Value: "true")}),
            new (
                    Id: "ABC03",
                    Name: "Site 3",
                    Address: "3 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [.506, 65]),
                AttributeValues: new List<AttributeValue>() {new AttributeValue(Id: "accessibility/access_need_1", Value: "true")}),
            new (
                    Id: "ABC01",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [.505, 65]),
                AttributeValues: new List<AttributeValue>() {new AttributeValue(Id: "accessibility/access_need_1", Value: "true")})
        };

        var expectedSites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(
                Id: "ABC01",
                Name: "Site 1",
                Address: "1 Park Row",
                PhoneNumber: "0113 1111111",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                Location: new Location(Type: "Point", Coordinates: [.505, 65.0]),
                AttributeValues: new List<AttributeValue>() {new AttributeValue(Id: "accessibility/access_need_1", Value: "true")}),
                Distance: 234),
            new SiteWithDistance(new Site(
                Id: "ABC03",
                Name: "Site 3",
                Address: "3 Park Row",
                PhoneNumber: "0113 1111111",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                Location: new Location(Type: "Point", Coordinates: [.506, 65.0]),
                AttributeValues: new List<AttributeValue>() {new AttributeValue(Id: "accessibility/access_need_1", Value: "true")}),
                Distance: 281)
        };
        
        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites);

        var result = await _sut.FindSitesByArea(0.5, 65, 50000, 2, []);
        result.Should().BeEquivalentTo(expectedSites);
    }

    [Fact]
    public async Task FindSitesByArea_ReturnsSites_WithRequestedAccessNeeds()
    {
        var sites = new List<Site>()
        {
            new (
                    Id: "ABC01",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [.505, 50.0]),
                    AttributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_1", Value: "true")}),                
            new (
                    Id: "ABC02",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [.506, 50.0]),
                    AttributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_1", Value: "false")}),                
        };

        var expectedSites = new List<SiteWithDistance>()
        {
            new (new Site(
                    Id: "ABC01",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.505, 50.0]),
                    AttributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_1", Value: "true")}),
                Distance: 357),
        };

        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites);

        var result = await _sut.FindSitesByArea(0.5, 50, 50000, 50, ["access_need_1"]);
        result.Should().BeEquivalentTo(expectedSites);
    }

    [Fact]
    public async Task FindSitesByArea_ReturnsNoSites_IfNoAccessNeedMatchesAreFound()
    {
        var sites = new List<Site>()
        {
            new (
                    Id: "ABC01",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.0, 50.0]),
                    AttributeValues: new List<AttributeValue>() {new (Id: "accessibility/accessibility/access_need_1", Value: "false")}),                
            new (
                    Id: "ABC02",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [.05, 50.0]),
                    AttributeValues: new List<AttributeValue>() {new (Id: "accessibility/accessibility/access_need_1", Value: "false")}),
        };

        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites);

        var result = await _sut.FindSitesByArea(0.0, 50, 50000, 50, ["access_need_1"]);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindSitesByArea_ReturnSitesBasedOnDistanceAndMaxRecords_IfNoAccessNeedsAreRequested()
    {
        var sites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(
                    Id: "ABC01",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.04, 50.0]),
                    AttributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_1", Value: "true")}),
                Distance: 2858),
            new SiteWithDistance(new Site(
                    Id: "ABC02",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.05, 50.0]),
                    AttributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_2", Value: "true")}),
                Distance: 3573),
        };

        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites.Select(s => s.Site));
        var result = await _sut.FindSitesByArea(0.0, 50, 50000, 50, []);
        result.Should().BeEquivalentTo(sites);
    }

    [Fact]
    public async Task FindSitesByArea_DoesNotReturnSitesWithNoAttributeValues_IfAccessNeedsAreRequested()
    {
        var sites = new List<Site>()
        {
            new(
                    Id: "ABC01",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.0, 50.0]),
                    AttributeValues: Array.Empty<AttributeValue>()),
            new (
                    Id: "ABC02",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.1, 50.1]),
                    AttributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_2", Value: "true")})            
        };
        var expectedSites = new List<SiteWithDistance>()
        {
            new (new Site(
                    Id: "ABC02",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.1, 50.1]),
                    AttributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_2", Value: "true")}),
                Distance: 13213),
        };
        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites);
        var result = await _sut.FindSitesByArea(0.0, 50, 50000, 50, ["access_need_2"]);
        result.Should().BeEquivalentTo(expectedSites);
    }

    [Fact]
    public async Task FindSitesByArea_ReturnsSites_IfRequestedAccessNeedsAreEmpty()
    {
        var sites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(
                    Id: "ABC01",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.04, 50.0]),
                    AttributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_1", Value: "true")}),
                Distance: 2858),
            new SiteWithDistance(new Site(
                    Id: "ABC02",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.05, 50.0]),
                    AttributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_1", Value: "false")}),
                Distance: 3573),
        };
        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites.Select(s => s.Site));
        var result = await _sut.FindSitesByArea(0.0, 50, 50000, 50, [""]);
        result.Should().BeEquivalentTo(sites);
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
        var sites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(
                    Id: "ABC01",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.04, 50.0]),
                    AttributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_1", Value: "true")}),
                Distance: 2858),
            new SiteWithDistance(new Site(
                    Id: "ABC02",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.05, 50.0]),
                    AttributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_1", Value: "false")}),
                Distance: 3573),
        };
        object? outSites = sites.Select(s => s.Site);
        _memoryCache.Setup(x => x.TryGetValue("sites", out outSites)).Returns(true);
        var result = await _sut.FindSitesByArea(0.0, 50, 50000, 50, [""]);
        result.Should().BeEquivalentTo(sites);
        _siteStore.Verify(x => x.GetAllSites(), Times.Never());
    }

    [Fact]
    public async Task FindSitesByArea_IgnoresCache_WhenRequiredTo()
    {
        var sites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(
                    Id: "ABC01",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.04, 50.0]),
                    AttributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_1", Value: "true")}),
                Distance: 2858),
            new SiteWithDistance(new Site(
                    Id: "ABC02",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.05, 50.0]),
                    AttributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_1", Value: "false")}),
                Distance: 3573),
        };
        object? outSites = sites.Take(1).Select(s => s.Site);
        _memoryCache.Setup(x => x.TryGetValue("sites", out outSites)).Returns(true);
        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites.Select(s => s.Site));

        var result = await _sut.FindSitesByArea(0.0, 50, 50000, 50, [""], true);
        result.Should().BeEquivalentTo(sites);
        _siteStore.Verify(x => x.GetAllSites(), Times.Once);
    }

    [Fact]
    public async Task FindSitesByArea_WritesToCache_AfterCacheMiss()
    {
        var sites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(
                    Id: "ABC01",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.04, 50.0]),
                    AttributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_1", Value: "true")}),
                Distance: 2858),
            new SiteWithDistance(new Site(
                    Id: "ABC02",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.05, 50.0]),
                    AttributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_1", Value: "false")}),
                Distance: 3573),
        };
        object? outSites = null;
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
        const string siteId = "ABC01";
        var site = new Site(
            Id: siteId,
            Name: "Site 1",
            Address: "1 Park Row",
            PhoneNumber: "0113 1111111",
            Region: "R1",
            IntegratedCareBoard: "ICB1",
            Location: new Location(Type: "Point", Coordinates: [2.0, 70.0]),
            AttributeValues: new List<AttributeValue>() { new AttributeValue(Id: "Attribute 1", Value: "true") });

        var expectedSite = new Site(
            Id: siteId,
            Name: "Site 1",
            Address: "1 Park Row",
            PhoneNumber: "0113 1111111",
            Region: "R1",
            IntegratedCareBoard: "ICB1",
            Location: new Location(Type: "Point", Coordinates: [2.0, 70.0]),
            AttributeValues: new List<AttributeValue>() { new AttributeValue(Id: "Attribute 1", Value: "true") });
        _siteStore.Setup(x => x.GetSiteById("ABC01")).ReturnsAsync(site);

        var result = await _sut.GetSiteByIdAsync(siteId);
        result.Should().BeEquivalentTo(expectedSite);
    }

    [Fact]
    public async Task GetSiteByIdAsync_ReturnsRequestedSite_AndFiltersAttributesByScope()
    {
        const string siteId = "ABC01";
        var site = new Site(
            Id: siteId,
            Name: "Site 1",
            Address: "1 Park Row",
            PhoneNumber: "0113 1111111",
            Region: "R1",
            IntegratedCareBoard: "ICB1",
            Location: new Location(Type: "Point", Coordinates: [2.0, 70.0]),
            AttributeValues: [
                new AttributeValue(Id: "test_scope/Attribute 1", Value: "true"),
                new AttributeValue(Id: "Attribute 2", Value: "true"),
                new AttributeValue(Id: "test_scope/Attribute 3", Value: "true"),
            ]);

        var expectedSite = new Site(
            Id: siteId,
            Name: "Site 1",
            Address: "1 Park Row",
            PhoneNumber: "0113 1111111", Region: "R1",
            IntegratedCareBoard: "ICB1",
            Location: new Location(Type: "Point", Coordinates: [2.0, 70.0]),
            AttributeValues: [
                new AttributeValue(Id: "test_scope/Attribute 1", Value: "true"),
                new AttributeValue(Id: "test_scope/Attribute 3", Value: "true"),
            ]);
        _siteStore.Setup(x => x.GetSiteById("ABC01")).ReturnsAsync(site);

        var result = await _sut.GetSiteByIdAsync(siteId, "test_scope");

        result.Should().BeEquivalentTo(expectedSite);
    }

    [Theory]
    [InlineData("*")]
    [InlineData("site_details")]
    public async Task GetSiteByIdAsync_ReturnsDefault_WhenSiteIsNotFound(string scope)
    {
        const string siteId = "ABC01";
        _siteStore.Setup(x => x.GetSiteById(siteId)).ReturnsAsync((Site)null!);

        var result = await _sut.GetSiteByIdAsync(siteId, scope);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllSites_ReturnsAllSites()
    {
        var sites = new List<Site>()
        {
            new Site(
                    Id: "ABC01",
                    Name: "Site 1",
                    Address: "1 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.04, 50.0]),
                    AttributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_1", Value: "true")}),
            new Site(
                    Id: "ABC02",
                    Name: "Site 2",
                    Address: "2 Park Row",
                    PhoneNumber: "0113 1111111",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    Location: new Location(Type: "Point", Coordinates: [0.05, 50.0]),
                    AttributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_1", Value: "false")})
        };

        _siteStore.Setup(x => x.GetAllSites()).ReturnsAsync(sites);

        var result = await _sut.GetAllSites();

        result.Count().Should().Be(sites.Count());
    }
}
