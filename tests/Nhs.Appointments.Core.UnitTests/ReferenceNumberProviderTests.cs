namespace Nhs.Appointments.Core.UnitTests;

public class ReferenceNumberProviderTests
{
    private readonly ReferenceNumberProvider _sut;
    private readonly Mock<IReferenceNumberDocumentStore> _referenceNumberDocumentStore = new();
    private readonly Mock<TimeProvider> _timeProvider = new();

    public ReferenceNumberProviderTests()
    {
        _sut = new ReferenceNumberProvider(_referenceNumberDocumentStore.Object, _timeProvider.Object);
    }    

    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2077, 1, 31, 9, 0, 59));
        _referenceNumberDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(2345000);        

        var result = await _sut.GetReferenceNumber("test");
        result.Should().Be("002-90-345000");
    }
}
