namespace Nhs.Appointments.Core.UnitTests;
public class SiteServiceTests
{
    private readonly SiteService _sut;
    private readonly Mock<ISiteStore> _siteStore = new();
    
    public SiteServiceTests()
    {
        _sut = new SiteService(_siteStore.Object);

    }
    
    [Fact]
    public async Task FindSitesByArea_ReturnsSitesOrderByDistance_InAscendingOrder()
    {
        var sites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(
                Id: "ABC02", 
                Name: "Site 2", 
                Address: "2 Park Row", 
                Location: new Location(Type: "Point", Coordinates: [2.0, 70.0]), 
                AttributeValues: new List<AttributeValue>() {new AttributeValue(Id: "Attribute 1", Value: "true")}), 
                Distance: 5000),
            new SiteWithDistance(new Site(
                Id: "ABC03", 
                Name: "Site 3", 
                Address: "3 Park Row", 
                Location: new Location(Type: "Point", Coordinates: [1.0, 60.0]),
                AttributeValues: new List<AttributeValue>() {new AttributeValue(Id: "Attribute 1", Value: "true")}), 
                Distance: 3000),
            new SiteWithDistance(new Site(
                Id: "ABC01", 
                Name: "Site 1", 
                Address: "1 Park Row", 
                Location: new Location(Type: "Point", Coordinates: [0.0, 50.0]),
                AttributeValues: new List<AttributeValue>() {new AttributeValue(Id: "Attribute 1", Value: "true")}),
                Distance: 1000)
        };
        
        var expectedSites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(
                Id: "ABC01", 
                Name: "Site 1", 
                Address: "1 Park Row",
                Location: new Location(Type: "Point", Coordinates: [0.0, 50.0]),
                AttributeValues: new List<AttributeValue>() {new AttributeValue(Id: "Attribute 1", Value: "true")}),
                Distance: 1000),
            new SiteWithDistance(new Site(
                Id: "ABC03", 
                Name: "Site 3", 
                Address: "3 Park Row", 
                Location: new Location(Type: "Point", Coordinates: [1.0, 60.0]),
                AttributeValues: new List<AttributeValue>() {new AttributeValue(Id: "Attribute 1", Value: "true")}),
                Distance: 3000),
            new SiteWithDistance(new Site(
                Id: "ABC02", 
                Name: "Site 2", 
                Address: "2 Park Row", 
                Location: new Location(Type: "Point", Coordinates: [2.0, 70.0]),
                AttributeValues: new List<AttributeValue>() {new AttributeValue(Id: "Attribute 1", Value: "true")}),
                Distance: 5000)
        };
        
        _siteStore.Setup(x => x.GetSitesByArea(0.5, 65, 50000)).ReturnsAsync(sites);
        
        var result = await _sut.FindSitesByArea(0.5, 65, 50000, 50, ["access_need_1"]);
        result.Should().BeEquivalentTo(expectedSites);
    }
    
    [Fact]
    public async Task FindSitesByArea_ReturnsRequestedNumberOfSites()
    {
        var sites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(
                Id: "ABC02", 
                Name: "Site 2", 
                Address: "2 Park Row", 
                Location: new Location(Type: "Point", Coordinates: [2.0, 70.0]),
                AttributeValues: new List<AttributeValue>() {new AttributeValue(Id: "Attribute 1", Value: "true")}), 
                Distance: 5000),
            new SiteWithDistance(new Site(
                Id: "ABC03", 
                Name: "Site 3", 
                Address: "3 Park Row", 
                Location: new Location(Type: "Point", Coordinates: [1.0, 60.0]),
                AttributeValues: new List<AttributeValue>() {new AttributeValue(Id: "Attribute 1", Value: "true")}), 
                Distance: 3000),
            new SiteWithDistance(new Site(
                Id: "ABC01", 
                Name: "Site 1", 
                Address: "1 Park Row", 
                Location: new Location(Type: "Point", Coordinates: [0.0, 50.0]),
                AttributeValues: new List<AttributeValue>() {new AttributeValue(Id: "Attribute 1", Value: "true")}), 
                Distance: 1000)
        };
        
        var expectedSites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(
                Id: "ABC01", 
                Name: "Site 1", 
                Address: "1 Park Row", 
                Location: new Location(Type: "Point", Coordinates: [0.0, 50.0]),
                AttributeValues: new List<AttributeValue>() {new AttributeValue(Id: "Attribute 1", Value: "true")}),
                Distance: 1000),
            new SiteWithDistance(new Site(
                Id: "ABC03", 
                Name: "Site 3", 
                Address: "3 Park Row", 
                Location: new Location(Type: "Point", Coordinates: [1.0, 60.0]),
                AttributeValues: new List<AttributeValue>() {new AttributeValue(Id: "Attribute 1", Value: "true")}), 
                Distance: 3000)
        };
        
        _siteStore.Setup(x => x.GetSitesByArea(0.5, 65, 50000)).ReturnsAsync(sites);
        
        var result = await _sut.FindSitesByArea(0.5, 65, 50000, 2, ["access_need_1"]);
        result.Should().BeEquivalentTo(expectedSites);
    }
    
    [Fact]
    public async Task FindSitesByArea_ReturnsEmptyCollection_WhenNoSitesAreFound()
    {
        var sites = new List<SiteWithDistance>();
        
        _siteStore.Setup(x => x.GetSitesByArea(0.5, 65, 50000)).ReturnsAsync(sites);
        
        var result = await _sut.FindSitesByArea(0.5, 65, 50000, 2, ["access_need_1"]);
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetSiteByIdAsync_ReturnsRequestedSite()
    {
        const string siteId = "ABC01";
        var site = new Site(
            Id: siteId, 
            Name: "Site 1", 
            Address: "1 Park Row", 
            Location: new Location(Type: "Point", Coordinates: [2.0, 70.0]),
            AttributeValues: new List<AttributeValue>() {new AttributeValue(Id: "Attribute 1", Value: "true")});
        
        var expectedSite = new Site(
            Id: siteId, 
            Name: "Site 1", 
            Address: "1 Park Row", 
            Location: new Location(Type: "Point", Coordinates: [2.0, 70.0]),
            AttributeValues: new List<AttributeValue>() {new AttributeValue(Id: "Attribute 1", Value: "true")});        
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
