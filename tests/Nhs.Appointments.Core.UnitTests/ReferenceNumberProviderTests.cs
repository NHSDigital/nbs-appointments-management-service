namespace Nhs.Appointments.Core.UnitTests;

public class ReferenceNumberProviderTests
{
    private readonly ReferenceNumberProvider _sut;
    private readonly Mock<ISiteConfigurationStore> _siteConfigurationStore = new();
    private readonly Mock<IReferenceNumberDocumentStore> _referenceNumberDocumentStore = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();

    public ReferenceNumberProviderTests()
    {
        _sut = new ReferenceNumberProvider(_siteConfigurationStore.Object, _referenceNumberDocumentStore.Object, _dateTimeProvider.Object);
    }

    [Fact]
    public async Task GetReferenceNumber_AssignsRefGroup_WhenNotCurrentAssigned()
    {
        var siteConfiguration = new SiteConfiguration
        {
            SiteId = "test"
        };

        _siteConfigurationStore.Setup(x => x.GetAsync("test")).ReturnsAsync(siteConfiguration);
        _referenceNumberDocumentStore.Setup(x => x.AssignReferenceGroup()).ReturnsAsync(14);

        await _sut.GetReferenceNumber("test");

        _referenceNumberDocumentStore.Verify(x => x.AssignReferenceGroup(), Times.Once());
        _siteConfigurationStore.Verify(x => x.AssignPrefix("test", 14), Times.Once());
    }

    [Fact]
    public async Task GetReferenceNumber_DoesNotAssignsRefGroup_WhenAlreadyAssigned()
    {
        var siteConfiguration = new SiteConfiguration
        {
            SiteId = "test",
            ReferenceNumberGroup = 14
        };

        _siteConfigurationStore.Setup(x => x.GetAsync("test")).ReturnsAsync(siteConfiguration);        

        await _sut.GetReferenceNumber("test");

        _referenceNumberDocumentStore.Verify(x => x.AssignReferenceGroup(), Times.Never);
        _siteConfigurationStore.Verify(x => x.AssignPrefix("test", It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber()
    {
        _dateTimeProvider.Setup(x => x.Now).Returns(new DateTime(2077, 1, 31, 9, 0, 59));
        _referenceNumberDocumentStore.Setup(x => x.GetNextSequenceNumber(14)).ReturnsAsync(2345);

        var siteConfiguration = new SiteConfiguration
        {
            SiteId = "test",
            ReferenceNumberGroup = 14
        };

        _siteConfigurationStore.Setup(x => x.GetAsync("test")).ReturnsAsync(siteConfiguration);

        var result = await _sut.GetReferenceNumber("test");
        result.Should().Be("14-90-002345");
    }
}
