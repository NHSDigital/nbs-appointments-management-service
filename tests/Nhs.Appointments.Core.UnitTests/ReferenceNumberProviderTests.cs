using System.Collections;
using Microsoft.Extensions.Caching.Memory;

namespace Nhs.Appointments.Core.UnitTests;

public class ReferenceNumberProviderTests
{
    //TODO store elsewhere!!
    private static readonly byte[] HmacSecretKey =
    [
        0x00, 0x01, 0x02, 0x03,
        0x04, 0x05, 0x06, 0x07,
        0x08, 0x09, 0x0A, 0x0B,
        0x0C, 0x0D, 0x0E, 0x0F
    ];

    private readonly Mock<IBookingReferenceDocumentStore> _bookingReferenceDocumentStore = new();
    private readonly Mock<TimeProvider> _timeProvider = new();

    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 5, 31, 9, 0, 59));
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(2345123);

        using var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new ReferenceNumberProvider(_bookingReferenceDocumentStore.Object, cache, _timeProvider.Object);
        
        var result = await sut.GetReferenceNumber(HmacSecretKey);
        result.Should().Be("3825-0234-5123");
    }

    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_MinDate()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(1970, 1, 1, 00, 00, 00));
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(1);

        using var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new ReferenceNumberProvider(_bookingReferenceDocumentStore.Object, cache, _timeProvider.Object);
        
        var result = await sut.GetReferenceNumber(HmacSecretKey);
        result.Should().Be("0170-0000-0001");
    }

    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_FirstTwoDigitsIncrementEveryFourDays()
    {
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(74639103);
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

            using var cache = new MemoryCache(new MemoryCacheOptions());
            var sut = new ReferenceNumberProvider(_bookingReferenceDocumentStore.Object, cache, _timeProvider.Object);
            
            var result = await sut.GetReferenceNumber(HmacSecretKey);
            result.Should().Be($"{dayStep:00}25-7463-9103");
        }
    }

    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_MaxDate_LeapYear()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2024, 12, 31, 23, 59, 59));
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(99999999);

        using var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new ReferenceNumberProvider(_bookingReferenceDocumentStore.Object, cache, _timeProvider.Object);
        
        var result = await sut.GetReferenceNumber(HmacSecretKey);
        result.Should().Be("9224-9999-9999");
    }

    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_MinSequenceNumber_0()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2077, 1, 01, 17, 23, 00));
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(0);

        using var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new ReferenceNumberProvider(_bookingReferenceDocumentStore.Object, cache, _timeProvider.Object);
        
        var result = await sut.GetReferenceNumber(HmacSecretKey);
        result.Should().Be("0177-0000-0000");
    }

    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_MinSequenceNumber_1()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2077, 1, 01, 17, 23, 00));
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(1);

        using var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new ReferenceNumberProvider(_bookingReferenceDocumentStore.Object, cache, _timeProvider.Object);
        
        var result = await sut.GetReferenceNumber(HmacSecretKey);
        result.Should().Be("0177-0000-0001");
    }

    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_SameSequenceNumber_DifferentWeek()
    {
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(356035);
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 7, 13, 17, 23, 00));
        
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new ReferenceNumberProvider(_bookingReferenceDocumentStore.Object, cache, _timeProvider.Object);
        
        var firstResult = await sut.GetReferenceNumber(HmacSecretKey);
        firstResult.Should().Be("4925-0035-6035");

        //sequence has passed 100 million and reset
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(356035 + 100000000);
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2026, 2, 04, 17, 23, 00));
        var secondResult = await sut.GetReferenceNumber(HmacSecretKey);
        secondResult.Should().Be("0926-0035-6035");
    }

    /// <summary>
    ///     If MYA is still in use in 100 years time, this will need addressing...
    /// </summary>
    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_PotentialCollisionAfterAHundredYears()
    {
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(93451340);
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 1, 01, 17, 23, 00));
        
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new ReferenceNumberProvider(_bookingReferenceDocumentStore.Object, cache, _timeProvider.Object);
        
        var normalResult = await sut.GetReferenceNumber(HmacSecretKey);

        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2125, 1, 01, 17, 23, 00));
        var hundredYearsResult = await sut.GetReferenceNumber(HmacSecretKey);

        hundredYearsResult.Should().Be(normalResult);
    }

    [Fact(Skip = "Very slow test! Can change the value of SequenceMax to something smaller if want a full run")]
    // [Fact]
    public async Task Generates_Unique_References_Within_Partition()
    {
        var generatedReferences = new HashSet<long>();
        
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(DateTimeOffset.UtcNow.AddDays(67));
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var sut = new ReferenceNumberProvider(_bookingReferenceDocumentStore.Object, cache, _timeProvider.Object);

        for (var i = 0; i < ReferenceNumberProvider.SequenceMax; i++)
        {
            _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(i + 1);
            var referenceNumber = await sut.GetReferenceNumber(HmacSecretKey);
            
            var digitReference = long.Parse(referenceNumber.Replace("-", string.Empty));
            
            generatedReferences.Add(digitReference);
        }

        //if the distinct list of generatedReferences is equal to its own count, that means it is a unique list
        Assert.Equal(generatedReferences.Distinct().Count(), generatedReferences.Count);

        // every value 0..N-1 was hit exactly once
        Assert.Equal(ReferenceNumberProvider.SequenceMax, generatedReferences.Count);
    }
}
