namespace Nhs.Appointments.Core.UnitTests;

public class ReferenceNumberProviderTests
{
    private readonly ReferenceNumberProvider _sut;
    private readonly Mock<ISiteStore> _siteStore = new();
    private readonly Mock<IReferenceNumberDocumentStore> _referenceNumberDocumentStore = new();
    private readonly Mock<TimeProvider> _timeProvider = new();

    public ReferenceNumberProviderTests()
    {
        _sut = new ReferenceNumberProvider(_siteStore.Object, _referenceNumberDocumentStore.Object, _timeProvider.Object);
    }

    [Fact]
    public async Task GetReferenceNumber_AssignsRefGroup_WhenNotCurrentAssigned()
    {
        _siteStore.Setup(x => x.GetReferenceNumberGroup("test")).ReturnsAsync(0);
        _referenceNumberDocumentStore.Setup(x => x.AssignReferenceGroup()).ReturnsAsync(14);

        await _sut.GetReferenceNumber("test");

        _referenceNumberDocumentStore.Verify(x => x.AssignReferenceGroup(), Times.Once());
        _siteStore.Verify(x => x.AssignPrefix("test", 14), Times.Once());
    }

    [Fact]
    public async Task GetReferenceNumber_DoesNotAssignsRefGroup_WhenAlreadyAssigned()
    {
        _siteStore.Setup(x => x.GetReferenceNumberGroup("test")).ReturnsAsync(14);

        await _sut.GetReferenceNumber("test");

        _referenceNumberDocumentStore.Verify(x => x.AssignReferenceGroup(), Times.Never);
        _siteStore.Verify(x => x.AssignPrefix("test", It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2077, 1, 31, 9, 0, 59));
        _referenceNumberDocumentStore.Setup(x => x.GetNextSequenceNumber(14)).ReturnsAsync(2345);

        _siteStore.Setup(x => x.GetReferenceNumberGroup("test")).ReturnsAsync(14);

        var result = await _sut.GetReferenceNumber("test");
        result.Should().Be("14-90-002345");
    }
}
