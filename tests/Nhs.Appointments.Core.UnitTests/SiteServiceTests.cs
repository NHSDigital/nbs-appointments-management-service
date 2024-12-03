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
    public async Task FindSitesByArea_ReturnsSitesOrderedByDistance_InAscendingOrder()
    {
        var sites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(
                    id: "ABC02", 
                    name: "Site 2", 
                    address: "2 Park Row", 
                    phoneNumber: "0113 1111111",
                    region: "R1",
                    integratedCareBoard: "ICB1",
                    location: new Location(Type: "Point", Coordinates: [2.0, 70.0]), 
                    attributeValues: new List<AttributeValue>() {new AttributeValue(Id: "accessibility/access_need_1", Value: "true")}), 
                Distance: 5000),
            new SiteWithDistance(new Site(
                    id: "ABC03", 
                    name: "Site 3", 
                    address: "3 Park Row", 
                    phoneNumber: "0113 1111111",
                    region: "R1",
                    integratedCareBoard: "ICB1",
                    location: new Location(Type: "Point", Coordinates: [1.0, 60.0]),
                attributeValues: new List<AttributeValue>() {new AttributeValue(Id: "accessibility/access_need_1", Value: "true")}), 
                Distance: 3000),
            new SiteWithDistance(new Site(
                    id: "ABC01", 
                    name: "Site 1", 
                    address: "1 Park Row",
                    phoneNumber: "0113 1111111",
                    region: "R1",
                    integratedCareBoard: "ICB1",
                    location: new Location(Type: "Point", Coordinates: [0.0, 50.0]),
                attributeValues: new List<AttributeValue>() {new AttributeValue(Id: "accessibility/access_need_1", Value: "true")}),
                Distance: 1000)
        };
        
        var expectedSites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(
                    id: "ABC01", 
                    name: "Site 1", 
                    address: "1 Park Row",
                    phoneNumber: "0113 1111111",
                    region: "R1",
                    integratedCareBoard: "ICB1",
                    location: new Location(Type: "Point", Coordinates: [0.0, 50.0]),
                attributeValues: new List<AttributeValue>() {new AttributeValue(Id: "accessibility/access_need_1", Value: "true")}),
                Distance: 1000),
            new SiteWithDistance(new Site(
                    id: "ABC03", 
                    name: "Site 3", 
                    address: "3 Park Row", 
                    phoneNumber: "0113 1111111",
                    region: "R1",
                    integratedCareBoard: "ICB1",
                    location: new Location(Type: "Point", Coordinates: [1.0, 60.0]),
                attributeValues: new List<AttributeValue>() {new AttributeValue(Id: "accessibility/access_need_1", Value: "true")}),
                Distance: 3000),
            new SiteWithDistance(new Site(
                    id: "ABC02", 
                    name: "Site 2", 
                    address: "2 Park Row",
                    phoneNumber: "0113 1111111",
                    region: "R1",
                    integratedCareBoard: "ICB1",
                    location: new Location(Type: "Point", Coordinates: [2.0, 70.0]),
                    attributeValues: new List<AttributeValue>() {new AttributeValue(Id: "accessibility/access_need_1", Value: "true")}),
                Distance: 5000)
        };
        
        _siteStore.Setup(x => x.GetSitesByArea(0.5, 65, 50000)).ReturnsAsync(sites);
        
        var result = await _sut.FindSitesByArea(0.5, 65, 50000, 50, []);
        result.Should().BeEquivalentTo(expectedSites);
    }
    
    [Fact]
    public async Task FindSitesByArea_ReturnsRequestedNumberOfSites()
    {
        var sites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(
                id: "ABC02", 
                name: "Site 2", 
                address: "2 Park Row", 
                phoneNumber: "0113 1111111",
                region: "R1",
                integratedCareBoard: "ICB1",
                location: new Location(Type: "Point", Coordinates: [2.0, 70.0]),
                attributeValues: new List<AttributeValue>() {new AttributeValue(Id: "accessibility/access_need_1", Value: "true")}), 
                Distance: 5000),
            new SiteWithDistance(new Site(
                id: "ABC03", 
                name: "Site 3", 
                address: "3 Park Row",
                phoneNumber: "0113 1111111",
                region: "R1",
                integratedCareBoard: "ICB1",
                location: new Location(Type: "Point", Coordinates: [1.0, 60.0]),
                attributeValues: new List<AttributeValue>() {new AttributeValue(Id: "accessibility/access_need_1", Value: "true")}), 
                Distance: 3000),
            new SiteWithDistance(new Site(
                id: "ABC01", 
                name: "Site 1", 
                address: "1 Park Row",
                phoneNumber: "0113 1111111",
                region: "R1",
                integratedCareBoard: "ICB1",
                location: new Location(Type: "Point", Coordinates: [0.0, 50.0]),
                attributeValues: new List<AttributeValue>() {new AttributeValue(Id: "accessibility/access_need_1", Value: "true")}), 
                Distance: 1000)
        };
        
        var expectedSites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(
                id: "ABC01", 
                name: "Site 1", 
                address: "1 Park Row",
                phoneNumber: "0113 1111111",
                region: "R1",
                integratedCareBoard: "ICB1",
                location: new Location(Type: "Point", Coordinates: [0.0, 50.0]),
                attributeValues: new List<AttributeValue>() {new AttributeValue(Id: "accessibility/access_need_1", Value: "true")}),
                Distance: 1000),
            new SiteWithDistance(new Site(
                id: "ABC03", 
                name: "Site 3", 
                address: "3 Park Row",
                phoneNumber: "0113 1111111",
                region: "R1",
                integratedCareBoard: "ICB1",
                location: new Location(Type: "Point", Coordinates: [1.0, 60.0]),
                attributeValues: new List<AttributeValue>() {new AttributeValue(Id: "accessibility/access_need_1", Value: "true")}), 
                Distance: 3000)
        };
        
        _siteStore.Setup(x => x.GetSitesByArea(0.5, 65, 50000)).ReturnsAsync(sites);
        
        var result = await _sut.FindSitesByArea(0.5, 65, 50000, 2, []);
        result.Should().BeEquivalentTo(expectedSites);
    }
    
    [Fact]
    public async Task FindSitesByArea_ReturnsSites_WithRequestedAccessNeeds()
    {
        var sites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(
                    id: "ABC01", 
                    name: "Site 1", 
                    address: "1 Park Row",
                    phoneNumber: "0113 1111111",
                    region: "R1",
                    integratedCareBoard: "ICB1",
                    location: new Location(Type: "Point", Coordinates: [0.0, 50.0]),
                    attributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_1", Value: "true")}),
                Distance: 1000),
            new SiteWithDistance(new Site(
                    id: "ABC02", 
                    name: "Site 2", 
                    address: "2 Park Row",
                    phoneNumber: "0113 1111111",
                    region: "R1",
                    integratedCareBoard: "ICB1",
                    location: new Location(Type: "Point", Coordinates: [1.0, 60.0]),
                    attributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_1", Value: "false")}), 
                Distance: 3000),
        };
        
        var expectedSites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(
                    id: "ABC01", 
                    name: "Site 1", 
                    address: "1 Park Row",
                    phoneNumber: "0113 1111111",
                    region: "R1",
                    integratedCareBoard: "ICB1",
                    location: new Location(Type: "Point", Coordinates: [0.0, 50.0]),
                    attributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_1", Value: "true")}),
                Distance: 1000),
        };
        
        _siteStore.Setup(x => x.GetSitesByArea(0.0, 50.0, 50000)).ReturnsAsync(sites);
        
        var result = await _sut.FindSitesByArea(0.0, 50, 50000, 50, ["access_need_1"]);
        result.Should().BeEquivalentTo(expectedSites);
    }
    
    [Fact]
    public async Task FindSitesByArea_ReturnsNoSites_IfNoAccessNeedMatchesAreFound()
    {
        var sites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(
                    id: "ABC01", 
                    name: "Site 1", 
                    address: "1 Park Row",
                    phoneNumber: "0113 1111111",
                    region: "R1",
                    integratedCareBoard: "ICB1",
                    location: new Location(Type: "Point", Coordinates: [0.0, 50.0]),
                    attributeValues: new List<AttributeValue>() {new (Id: "accessibility/accessibility/access_need_1", Value: "false")}),
                Distance: 1000),
            new SiteWithDistance(new Site(
                    id: "ABC02", 
                    name: "Site 2", 
                    address: "2 Park Row",
                    phoneNumber: "0113 1111111",
                    region: "R1",
                    integratedCareBoard: "ICB1",
                    location: new Location(Type: "Point", Coordinates: [1.0, 60.0]),
                    attributeValues: new List<AttributeValue>() {new (Id: "accessibility/accessibility/access_need_1", Value: "false")}), 
                Distance: 3000),
        };
        
        _siteStore.Setup(x => x.GetSitesByArea(0.0, 50.0, 50000)).ReturnsAsync(sites);
        
        var result = await _sut.FindSitesByArea(0.0, 50, 50000, 50, ["access_need_1"]);
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task FindSitesByArea_ReturnSitesBasedOnDistanceAndMaxRecords_IfNoAccessNeedsAreRequested()
    {
        var sites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(
                    id: "ABC01", 
                    name: "Site 1", 
                    address: "1 Park Row",
                    phoneNumber: "0113 1111111",
                    region: "R1",
                    integratedCareBoard: "ICB1",
                    location: new Location(Type: "Point", Coordinates: [0.0, 50.0]),
                    attributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_1", Value: "true")}),
                Distance: 1000),
            new SiteWithDistance(new Site(
                    id: "ABC02", 
                    name: "Site 2", 
                    address: "2 Park Row",
                    phoneNumber: "0113 1111111",
                    region: "R1",
                    integratedCareBoard: "ICB1",
                    location: new Location(Type: "Point", Coordinates: [1.0, 60.0]),
                    attributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_2", Value: "true")}), 
                Distance: 3000),
        };
        
        _siteStore.Setup(x => x.GetSitesByArea(0.0, 50.0, 50000)).ReturnsAsync(sites);
        var result = await _sut.FindSitesByArea(0.0, 50, 50000, 50, []);
        result.Should().BeEquivalentTo(sites);
    }
    
    [Fact]
    public async Task FindSitesByArea_DoesNotReturnSitesWithNoAttributeValues_IfAccessNeedsAreRequested()
    {
        var sites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(
                    id: "ABC01", 
                    name: "Site 1", 
                    address: "1 Park Row",
                    phoneNumber: "0113 1111111",
                    region: "R1",
                    integratedCareBoard: "ICB1",
                    location: new Location(Type: "Point", Coordinates: [0.0, 50.0]),
                    attributeValues: Array.Empty<AttributeValue>()),
                Distance: 1000),
            new SiteWithDistance(new Site(
                    id: "ABC02", 
                    name: "Site 2", 
                    address: "2 Park Row",
                    phoneNumber: "0113 1111111",
                    region: "R1",
                    integratedCareBoard: "ICB1",
                    location: new Location(Type: "Point", Coordinates: [1.0, 60.0]),
                    attributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_2", Value: "true")}), 
                Distance: 3000),
        };
        var expectedSites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(
                    id: "ABC02", 
                    name: "Site 2", 
                    address: "2 Park Row",
                    phoneNumber: "0113 1111111",
                    region: "R1",
                    integratedCareBoard: "ICB1",
                    location: new Location(Type: "Point", Coordinates: [1.0, 60.0]),
                    attributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_2", Value: "true")}), 
                Distance: 3000),
        };
        _siteStore.Setup(x => x.GetSitesByArea(0.0, 50.0, 50000)).ReturnsAsync(sites);
        var result = await _sut.FindSitesByArea(0.0, 50, 50000, 50, ["access_need_2"]);
        result.Should().BeEquivalentTo(expectedSites);
    }
    
    [Fact]
    public async Task FindSitesByArea_ReturnsSites_IfRequestedAccessNeedsAreEmpty()
    {
        var sites = new List<SiteWithDistance>()
        {
            new SiteWithDistance(new Site(
                    id: "ABC01", 
                    name: "Site 1", 
                    address: "1 Park Row",
                    phoneNumber: "0113 1111111",
                    region: "R1",
                    integratedCareBoard: "ICB1",
                    location: new Location(Type: "Point", Coordinates: [0.0, 50.0]),
                    attributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_1", Value: "true")}),
                Distance: 1000),
            new SiteWithDistance(new Site(
                    id: "ABC02", 
                    name: "Site 2", 
                    address: "2 Park Row", 
                    phoneNumber: "0113 1111111",
                    region: "R1",
                    integratedCareBoard: "ICB1",
                    location: new Location(Type: "Point", Coordinates: [0.1, 51.0]),
                    attributeValues: new List<AttributeValue>() {new (Id: "accessibility/access_need_1", Value: "false")}), 
                Distance: 3000),
        };
        _siteStore.Setup(x => x.GetSitesByArea(0.0, 50.0, 50000)).ReturnsAsync(sites);
        var result = await _sut.FindSitesByArea(0.0, 50, 50000, 50, [""]);
        result.Should().BeEquivalentTo(sites);
    }
    
    [Fact]
    public async Task FindSitesByArea_ReturnsEmptyCollection_WhenNoSitesAreFound()
    {
        var sites = Array.Empty<SiteWithDistance>();
        _siteStore.Setup(x => x.GetSitesByArea(0.5, 65, 50000)).ReturnsAsync(sites);
        var result = await _sut.FindSitesByArea(0.5, 65, 50000, 2, ["access_need_1"]);
        result.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetSiteByIdAsync_ReturnsRequestedSite()
    {
        const string siteId = "ABC01";
        var site = new Site(
            id: siteId, 
            name: "Site 1", 
            address: "1 Park Row",
            phoneNumber: "0113 1111111",
            region: "R1",
            integratedCareBoard: "ICB1",
            location: new Location(Type: "Point", Coordinates: [2.0, 70.0]),
            attributeValues: new List<AttributeValue>() {new AttributeValue(Id: "Attribute 1", Value: "true")});
        
        var expectedSite = new Site(
            id: siteId, 
            name: "Site 1", 
            address: "1 Park Row",
            phoneNumber: "0113 1111111",
            region: "R1",
            integratedCareBoard: "ICB1",
            location: new Location(Type: "Point", Coordinates: [2.0, 70.0]),
            attributeValues: new List<AttributeValue>() {new AttributeValue(Id: "Attribute 1", Value: "true")});
        _siteStore.Setup(x => x.GetSiteById("ABC01")).ReturnsAsync(site);
        
        var result = await _sut.GetSiteByIdAsync(siteId);
        result.Should().BeEquivalentTo(expectedSite);
    }

    [Fact]
    public async Task GetSiteByIdAsync_ReturnsRequestedSite_AndFiltersAttributesByScope()
    {
        const string siteId = "ABC01";
        var site = new Site(
            id: siteId,
            name: "Site 1",
            address: "1 Park Row",
            phoneNumber: "0113 1111111",
            region: "R1",
            integratedCareBoard: "ICB1",
            location: new Location(Type: "Point", Coordinates: [2.0, 70.0]),
            attributeValues: [
                new AttributeValue(Id: "test_scope/Attribute 1", Value: "true"),
                new AttributeValue(Id: "Attribute 2", Value: "true"),
                new AttributeValue(Id: "test_scope/Attribute 3", Value: "true"),
            ]);

        var expectedSite = new Site(
            id: siteId,
            name: "Site 1",
            address: "1 Park Row",
            phoneNumber: "0113 1111111",region: "R1",
            integratedCareBoard: "ICB1",
            location: new Location(Type: "Point", Coordinates: [2.0, 70.0]),
            attributeValues: [
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
}
