namespace Nhs.Appointments.Core.UnitTests;

public class ReferenceNumberProviderTests
{
    private readonly Mock<IBookingReferenceDocumentStore> _referenceNumberDocumentStore = new();
    private readonly ReferenceNumberProvider _sut;
    private readonly Mock<TimeProvider> _timeProvider = new();

    public ReferenceNumberProviderTests()
    {
        _sut = new ReferenceNumberProvider(_referenceNumberDocumentStore.Object, _timeProvider.Object);
    }

    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_MaxRng()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2077, 1, 31, 9, 0, 59));
        _referenceNumberDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(2345000);

        var result = await _sut.GetReferenceNumber();
        result.Should().Be("002-90-345000");
    }

    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_MinRng()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2077, 1, 1, 17, 23, 00));
        _referenceNumberDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(57167998);

        var result = await _sut.GetReferenceNumber();
        result.Should().Be("057-01-167998");
    }
    
    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_MaxSequenceNumber()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2077, 1, 07, 17, 23, 43));
        _referenceNumberDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(999999999);

        var result = await _sut.GetReferenceNumber();
        result.Should().Be("999-50-999999");
    }
    
    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_MinSequenceNumber()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2077, 1, 01, 17, 23, 00));
        _referenceNumberDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(1000000);

        var result = await _sut.GetReferenceNumber();
        result.Should().Be("001-01-000000");
    }
    
    /// <summary>
    /// Once the sequence increment passes 999,999,999; the logic fails and should throw an exception
    /// This should be managed closer to the time (likely to be in the very distant future!)
    /// </summary>
    [Fact]
    public async Task GetReferenceNumber_GeneratesException_SequenceNumberTooLarge()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2077, 1, 07, 17, 23, 43));
        
        _referenceNumberDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(1000000000);
        var exception = await Assert.ThrowsAsync<NotSupportedException>(async () => await _sut.GetReferenceNumber());
        
        exception.Message.Should().Contain("Booking reference generation is not supported for the provided sequence number: 1000000000");
    }
    
    /// <summary>
    /// If the sequence number is configured in the DB incorrectly, the logic fails and should throw an exception
    /// </summary>
    [Fact]
    public async Task GetReferenceNumber_GeneratesException_SequenceNumberTooSmall()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2077, 1, 07, 17, 23, 43));
        
        _referenceNumberDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(999999);
        var exception = await Assert.ThrowsAsync<NotSupportedException>(async () => await _sut.GetReferenceNumber());
        
        exception.Message.Should().Contain("Booking reference generation is not supported for the provided sequence number: 999999");
    }
}
