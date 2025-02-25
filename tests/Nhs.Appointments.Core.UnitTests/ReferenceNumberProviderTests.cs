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
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 5, 31, 9, 0, 59));
        _referenceNumberDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(2345123);

        var result = await _sut.GetReferenceNumber();
        result.Should().Be("3825-0234-5123");
    }

    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_MinDate()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(1970, 1, 1, 00, 00, 00));
        _referenceNumberDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(1);

        var result = await _sut.GetReferenceNumber();
        result.Should().Be("0170-0000-0001");
    }
    
    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_FirstTwoDigitsIncrementEveryFourDays()
    {
        _referenceNumberDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(74639103);
        var initialDate = new DateTime(2025, 1, 3, 0, 0, 0);

        var dayStep = 1;
        
        for (var i = 0; i < 362; i++)
        {
            if (i % 4 == 0)
            {
                dayStep++;
            }
            
            initialDate = initialDate.AddDays(1);
            _timeProvider.Setup(x => x.GetUtcNow()).Returns(initialDate);

            var result = await _sut.GetReferenceNumber();
            result.Should().Be($"{dayStep:00}25-7463-9103");
        }
    }
    
    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_MaxDate_LeapYear()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2024, 12, 31, 23, 59, 59));
        _referenceNumberDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(99999999);

        var result = await _sut.GetReferenceNumber();
        result.Should().Be("9224-9999-9999");
    }
    
    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_MinSequenceNumber_0()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2077, 1, 01, 17, 23, 00));
        _referenceNumberDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(0);

        var result = await _sut.GetReferenceNumber();
        result.Should().Be("0177-0000-0000");
    }
    
    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_MinSequenceNumber_1()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2077, 1, 01, 17, 23, 00));
        _referenceNumberDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(1);

        var result = await _sut.GetReferenceNumber();
        result.Should().Be("0177-0000-0001");
    }
    
    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_SameSequenceNumber_DifferentWeek()
    {
        _referenceNumberDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(356035);
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 7, 13, 17, 23, 00));
        var firstResult = await _sut.GetReferenceNumber();
        firstResult.Should().Be("4925-0035-6035");
        
        //sequence has passed 100 million and reset
        _referenceNumberDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(356035 + 100000000);
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2026, 2, 04, 17, 23, 00));
        var secondResult = await _sut.GetReferenceNumber();
        secondResult.Should().Be("0926-0035-6035");
    }
    
    /// <summary>
    /// If MYA is still in use in 100 years time, this will need addressing...
    /// </summary>
    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_PotentialCollisionAfterAHundredYears()
    {
        _referenceNumberDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(93451340);
        
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 1, 01, 17, 23, 00));
        var normalResult = await _sut.GetReferenceNumber();
        
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2125, 1, 01, 17, 23, 00));
        var hundredYearsResult = await _sut.GetReferenceNumber();
        
        hundredYearsResult.Should().Be(normalResult);
    }
}
