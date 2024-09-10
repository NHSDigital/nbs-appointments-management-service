namespace Nhs.Appointments.Core.UnitTests;
public class SiteSearchServiceTests
{
    private readonly SiteSearchService _sut;
    private readonly Mock<ISiteStore> _siteStore = new();
    
    public SiteSearchServiceTests()
    {
        _sut = new SiteSearchService(_siteStore.Object);

    }
    
    [Fact]
    public async Task FindSitesByArea_ReturnsSitesOrderByDistance_InAscendingOrder()
    {
        var sites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(Id: "ABC02", Name: "Site 2", Address: "2 Park Row", Location: new Location(Type: "Point", Coordinates: [2.0, 70.0])), Distance: 5000),
            new SiteWithDistance(new Site(Id: "ABC03", Name: "Site 3", Address: "3 Park Row", Location: new Location(Type: "Point", Coordinates: [1.0, 60.0])), Distance: 3000),
            new SiteWithDistance(new Site(Id: "ABC01", Name: "Site 1", Address: "1 Park Row", Location: new Location(Type: "Point", Coordinates: [0.0, 50.0])), Distance: 1000),
        };
        
        var expectedSites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(Id: "ABC01", Name: "Site 1", Address: "1 Park Row", Location: new Location(Type: "Point", Coordinates: [0.0, 50.0])), Distance: 1000),
            new SiteWithDistance(new Site(Id: "ABC03", Name: "Site 3", Address: "3 Park Row", Location: new Location(Type: "Point", Coordinates: [1.0, 60.0])), Distance: 3000),
            new SiteWithDistance(new Site(Id: "ABC02", Name: "Site 2", Address: "2 Park Row", Location: new Location(Type: "Point", Coordinates: [2.0, 70.0])), Distance: 5000),
        };
        
        _siteStore.Setup(x => x.GetSitesByArea(0.5, 65, 50000)).ReturnsAsync(sites);
        
        var result = await _sut.FindSitesByArea(0.5, 65, 50000, 50);
        result.Should().BeEquivalentTo(expectedSites);
    }
    
    [Fact]
    public async Task FindSitesByArea_ReturnsRequestedNumberOfSites()
    {
        var sites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(Id: "ABC02", Name: "Site 2", Address: "2 Park Row", Location: new Location(Type: "Point", Coordinates: [2.0, 70.0])), Distance: 5000),
            new SiteWithDistance(new Site(Id: "ABC03", Name: "Site 3", Address: "3 Park Row", Location: new Location(Type: "Point", Coordinates: [1.0, 60.0])), Distance: 3000),
            new SiteWithDistance(new Site(Id: "ABC01", Name: "Site 1", Address: "1 Park Row", Location: new Location(Type: "Point", Coordinates: [0.0, 50.0])), Distance: 1000)
        };
        
        var expectedSites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(Id: "ABC01", Name: "Site 1", Address: "1 Park Row", Location: new Location(Type: "Point", Coordinates: [0.0, 50.0])), Distance: 1000),
            new SiteWithDistance(new Site(Id: "ABC03", Name: "Site 3", Address: "3 Park Row", Location: new Location(Type: "Point", Coordinates: [1.0, 60.0])), Distance: 3000)
        };
        
        _siteStore.Setup(x => x.GetSitesByArea(0.5, 65, 50000)).ReturnsAsync(sites);
        
        var result = await _sut.FindSitesByArea(0.5, 65, 50000, 2);
        result.Should().BeEquivalentTo(expectedSites);
    }
    
    [Fact]
    public async Task FindSitesByArea_ReturnsEmptyCollection_WhenNoSitesAreFound()
    {
        var sites = new List<SiteWithDistance>();
        
        _siteStore.Setup(x => x.GetSitesByArea(0.5, 65, 50000)).ReturnsAsync(sites);
        
        var result = await _sut.FindSitesByArea(0.5, 65, 50000, 2);
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetSiteByIdAsync_ReturnsRequestedSite()
    {
        const string siteId = "ABC01";
        var site = new Site(Id: siteId, Name: "Site 1", Address: "1 Park Row", Location: new Location(Type: "Point", Coordinates: [2.0, 70.0]));
        
        var expectedSite = new Site(Id: siteId, Name: "Site 1", Address: "1 Park Row", Location: new Location(Type: "Point", Coordinates: [2.0, 70.0]));        
        _siteStore.Setup(x => x.GetSiteById("ABC01")).ReturnsAsync(site);
        
        var result = await _sut.GetSiteByIdAsync(siteId);
        result.Should().BeEquivalentTo(expectedSite);
    }
    
    [Fact]
    public async Task GetSiteByIdAsync_ReturnsDefault_WhenSiteIsNotFound()
    {
        const string siteId = "ABC01";
        _siteStore.Setup(x => x.GetSiteById(siteId)).ReturnsAsync((Site)null);
        
        var result = await _sut.GetSiteByIdAsync(siteId);
        result.Should().BeNull();
    }
}
