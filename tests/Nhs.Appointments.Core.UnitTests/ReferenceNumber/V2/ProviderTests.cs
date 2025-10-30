using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nhs.Appointments.Core.ReferenceNumber.V2;

namespace Nhs.Appointments.Core.UnitTests.ReferenceNumber.V2;

public class ProviderTests
{
    private static readonly byte[] HmacTestSecretKey =
    [
        0x00, 0x01, 0x02, 0x03,
        0x04, 0x05, 0x06, 0x07,
        0x08, 0x09, 0x0A, 0x0B,
        0x0C, 0x0D, 0x0E, 0x0F
    ];

    private readonly Mock<IBookingReferenceDocumentStore> _bookingReferenceDocumentStore = new();
    private readonly Mock<TimeProvider> _timeProvider = new();
    private readonly Mock<ILogger<Provider>> _logger = new();
    private readonly Mock<IOptions<ReferenceNumberOptions>> _options = new();

    private readonly IProvider _sut;

    public ProviderTests()
    {
        _options.Setup(x => x.Value).Returns(new ReferenceNumberOptions { HmacKeyVersion = 1, HmacKey = HmacTestSecretKey});
        _sut = new Provider(_options.Object, _bookingReferenceDocumentStore.Object, new MemoryCache(new MemoryCacheOptions()), _logger.Object, _timeProvider.Object);
    }
    
    [Fact]
    public void IsValidBookingReference_Supports_Existing_V1_ReferenceFormats()
    {
        Assert.True(_sut.IsValidBookingReference("14-25-002341"));
        Assert.True(_sut.IsValidBookingReference("56-36-843502"));
        Assert.True(_sut.IsValidBookingReference("99-99-999999"));
        Assert.True(_sut.IsValidBookingReference("00-00-000000"));
        
        Assert.True(_sut.IsValidBookingReference("14-90-002341"));
        Assert.True(_sut.IsValidBookingReference("14-90-002342"));
        Assert.True(_sut.IsValidBookingReference("14-90-002343"));
        Assert.True(_sut.IsValidBookingReference("14-90-002344"));
        Assert.True(_sut.IsValidBookingReference("14-90-002345"));
        Assert.True(_sut.IsValidBookingReference("14-90-002346"));
        Assert.True(_sut.IsValidBookingReference("14-90-002347"));
        Assert.True(_sut.IsValidBookingReference("14-90-002348"));
        Assert.True(_sut.IsValidBookingReference("14-90-002349"));
        Assert.True(_sut.IsValidBookingReference("14-90-002340"));
        
        Assert.False(_sut.IsValidBookingReference("14-a0-002340"));
        Assert.False(_sut.IsValidBookingReference("1b-70-002340"));
        Assert.False(_sut.IsValidBookingReference("14-70-0023c0"));
        
        Assert.False(_sut.IsValidBookingReference("142-90-002340"));
        Assert.False(_sut.IsValidBookingReference("14-903-002340"));
        Assert.False(_sut.IsValidBookingReference("14-90-0023406"));
        Assert.False(_sut.IsValidBookingReference("1-90-002340"));
        Assert.False(_sut.IsValidBookingReference("14-9-002340"));
        Assert.False(_sut.IsValidBookingReference("14-90-00340"));
        
        Assert.False(_sut.IsValidBookingReference("1490002345"));
        Assert.False(_sut.IsValidBookingReference("149000234"));
        Assert.False(_sut.IsValidBookingReference("14900023454"));
        Assert.False(_sut.IsValidBookingReference("1490-002345"));
        Assert.False(_sut.IsValidBookingReference("14-90002345"));
        Assert.False(_sut.IsValidBookingReference(" 1490002345 "));
        Assert.False(_sut.IsValidBookingReference(" 14-90-002345 "));
        Assert.False(_sut.IsValidBookingReference("14-90-002345 "));
        Assert.False(_sut.IsValidBookingReference(" 14-90-002345"));
    }

    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_Deterministically()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 5, 31, 9, 0, 59));
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(2345123);

        var result = await _sut.GetReferenceNumber();
        result.Should().Be("3825-69268-6774");
    }

    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_MinDate()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(1970, 1, 1, 00, 00, 00));
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(1);

        var result = await _sut.GetReferenceNumber();
        result.Should().Be("0170-73123-6791");
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

            var result = await _sut.GetReferenceNumber();

            Assert.True(_sut.IsValidBookingReference(result));
            result.Should().StartWith($"{dayStep:00}25");
        }
    }

    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_MaxDate_LeapYear()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2024, 12, 31, 23, 59, 59));
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(99999999);
        
        var result = await _sut.GetReferenceNumber();

        Assert.True(_sut.IsValidBookingReference(result));
        result.Should().Be("9224-44976-9071");
    }

    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_MinSequenceNumber_0()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2077, 1, 01, 17, 23, 00));
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(0);
        
        var result = await _sut.GetReferenceNumber();
        result.Should().Be("0177-00000-0006");
    }

    [Fact]
    public async Task GetReferenceNumber_LuhnChecksumDigitExists_MeansOnlyOneReferenceIsValid()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2016, 6, 06, 17, 23, 00));
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(76543789);

        var result = await _sut.GetReferenceNumber();
        result.Should().Be("4016-90927-6174");
        Assert.True(_sut.IsValidBookingReference(result));
        
        _logger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString().Contains("Booking Reference '4016-90927-6174' does not pass the valid Luhn digit requirement.")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Never
        );

        //the other 9 numbers with a different final digit should NOT be valid references
        Assert.False(_sut.IsValidBookingReference("4016-90927-6171"));
        Assert.False(_sut.IsValidBookingReference("4016-90927-6172"));
        Assert.False(_sut.IsValidBookingReference("4016-90927-6173"));
        Assert.False(_sut.IsValidBookingReference("4016-90927-6175"));
        Assert.False(_sut.IsValidBookingReference("4016-90927-6176"));
        Assert.False(_sut.IsValidBookingReference("4016-90927-6177"));
        Assert.False(_sut.IsValidBookingReference("4016-90927-6178"));
        Assert.False(_sut.IsValidBookingReference("4016-90927-6179"));
        Assert.False(_sut.IsValidBookingReference("4016-90927-6170"));
        
        VerifyLuhnDigitInvalidLogged("4016-90927-6171");
        VerifyLuhnDigitInvalidLogged("4016-90927-6172");
        VerifyLuhnDigitInvalidLogged("4016-90927-6173");
        VerifyLuhnDigitInvalidLogged("4016-90927-6175");
        VerifyLuhnDigitInvalidLogged("4016-90927-6176");
        VerifyLuhnDigitInvalidLogged("4016-90927-6177");
        VerifyLuhnDigitInvalidLogged("4016-90927-6178");
        VerifyLuhnDigitInvalidLogged("4016-90927-6179");
        VerifyLuhnDigitInvalidLogged("4016-90927-6170");
    }

    private void VerifyLuhnDigitInvalidLogged(string bookingReference)
    {
        _logger.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) =>
                    state.ToString().Contains($"Booking Reference '{bookingReference}' does not pass the valid Luhn digit requirement.")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once
        );
    }

    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_MinSequenceNumber_1()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2077, 1, 01, 17, 23, 00));
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(1);
        
        var result = await _sut.GetReferenceNumber();
        result.Should().Be("0177-40950-2016");
    }

    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_SameSequenceNumber_DifferentWeek()
    {
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(356035);
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 7, 13, 17, 23, 00));
        
        var firstResult = await _sut.GetReferenceNumber();
        firstResult.Should().Be("4925-52301-3450");

        //sequence has passed 100 million and reset
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(356035 + 100000000);
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 7, 17, 17, 23, 00));
        var secondResult = await _sut.GetReferenceNumber();
        secondResult.Should().Be("5025-82949-7652");
    }

    /// <summary>
    ///     If we somehow cross 100 million booking reference generations within an exact 4-day period, it would end up reusing
    ///     a reference.
    ///     Ideally this limit would never be hit, we are likely under a cyber-attack...
    /// </summary>
    [Fact]
    public async Task GetReferenceNumber_GeneratesSameReferenceNumber_SameSequenceNumber_WithinFourDays()
    {
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(356035);
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 7, 13, 17, 23, 00));

        var firstResult = await _sut.GetReferenceNumber();
        firstResult.Should().Be("4925-52301-3450");

        //sequence has passed 100 million and reset
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(356035 + 100000000);
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 7, 13, 17, 23, 00));
        var secondResult = await _sut.GetReferenceNumber();

        //NOT DESIRABLE!!
        secondResult.Should().Be(firstResult);
    }

    /// <summary>
    ///  If this value is ever changed/up-versioned the logic will need revisiting to ensure it still works.
    /// </summary>
    [Fact]
    public void SequenceMax_Should_Equal_Hundred_Million()
    {
        Provider.SequenceMax.Should().Be(100_000_000);
    }

    /// <summary>
    /// If this value is ever changed/up-versioned the logic will need revisiting to ensure it still works.
    /// </summary>
    [Fact]
    public void PartitionBucketLengthInDays_Should_Equal_Four()
    {
        Provider.PartitionBucketLengthInDays.Should().Be(4);
    }

    /// <summary>
    ///     It is possible, but unlikely, that two different hmac keys produce the same multiplier value for the same partition key
    ///     This is known and does not cause any issues with the logic.
    ///     The hard-coded data for this test that proves this scenario was found by trial and error iterations.
    /// </summary>
    [Fact]
    public void DeriveSequenceMultiplier_DifferentHmacKeys_CanProduce_SameMultiplier_For_SamePartitionKey()
    {
        const string samePartitionKey = "3125";
        
        byte[] hmacKey1 =
        [
            90,
            159,
            125,
            18,
            154,
            250,
            190,
            52,
            51,
            120,
            80,
            115,
            11,
            62,
            69,
            54,
            161,
            157,
            211,
            45,
            55,
            82,
            52,
            185,
            95,
            137,
            249,
            15,
            53,
            136,
            32,
            185
        ];

        byte[] hmacKey2 =
        [
            174,
            19,
            165,
            199,
            34,
            196,
            194,
            214,
            251,
            174,
            100,
            113,
            20,
            113,
            180,
            81,
            128,
            127,
            174,
            163,
            13,
            36,
            148,
            83,
            195,
            59,
            237,
            147,
            62,
            87,
            75,
            49
        ];

        _options.Setup(x => x.Value).Returns(new ReferenceNumberOptions { HmacKeyVersion = 1, HmacKey = hmacKey1});
        var multiplier1 = _sut.DeriveSequenceMultiplier(samePartitionKey);

        _options.Setup(x => x.Value).Returns(new ReferenceNumberOptions { HmacKeyVersion = 1, HmacKey = hmacKey2});
        var multiplier2 = _sut.DeriveSequenceMultiplier(samePartitionKey);

        //this is fine
        Assert.Equal(multiplier1, multiplier2);
    }

    /// <summary>
    ///     It is possible, but unlikely, that two different hmac keys produce the same multiplier value for the same partition key
    ///     This is known and does not cause any issues with the logic.
    ///     The hard-coded data for this test that proves this scenario was found by trial and error iterations.
    /// </summary>
    [Fact]
    public void DeriveSequenceMultiplier_DifferentHmacKeys_CanProduce_SameMultiplier_For_DifferentPartitionKeys()
    {
        byte[] hmacKey1 =
        [
            241,
            74,
            152,
            165,
            84,
            109,
            9,
            73,
            33,
            8,
            205,
            218,
            155,
            55,
            55,
            216,
            75,
            56,
            253,
            24,
            82,
            174,
            146,
            70,
            162,
            194,
            33,
            57,
            164,
            252,
            94,
            6
        ];
        
        _options.Setup(x => x.Value).Returns(new ReferenceNumberOptions { HmacKeyVersion = 1, HmacKey = hmacKey1});
        var partitionKey1 = "5728"; //around about October 15th 2028
        var multiplier1 = _sut.DeriveSequenceMultiplier(partitionKey1);
        
        byte[] hmacKey2 =
        [
            247,
            145,
            228,
            43,
            7,
            70,
            65,
            70,
            122,
            137,
            180,
            68,
            32,
            203,
            250,
            219,
            115,
            45,
            112,
            34,
            112,
            111,
            241,
            124,
            41,
            54,
            169,
            237,
            158,
            201,
            230,
            107
        ];
        
        _options.Setup(x => x.Value).Returns(new ReferenceNumberOptions { HmacKeyVersion = 1, HmacKey = hmacKey2});
        var partitionKey2 = "5837"; //around about August 20th 2037
        var multiplier2 = _sut.DeriveSequenceMultiplier(partitionKey2);

        Assert.Equal(multiplier1, multiplier2);
    }

    /// <summary>
    ///     It is possible, but unlikely, that two different partition keys produce the same multiplier value for the same hmac key
    ///     This is known and does not cause any issues with the logic.
    ///     The hard-coded data for this test that proves this scenario was found by trial and error iterations.
    /// </summary>
    [Fact]
    public void DeriveSequenceMultiplier_SameHmacKey_CanProduce_SameMultiplier_For_DifferentPartitionKeys()
    {
        var partitionKey1 = "1947"; //around about March 18th 2047
        var partitionKey2 = "2538"; //around about April 10th 2038

        var multiplier1 = _sut.DeriveSequenceMultiplier(partitionKey1);
        var multiplier2 = _sut.DeriveSequenceMultiplier(partitionKey2);

        //this is fine
        Assert.Equal(multiplier1, multiplier2);
    }

    /// <summary>
    ///     If MYA is still in use in 100 years time, this will need addressing...
    /// </summary>
    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_PotentialCollisionAfterAHundredYears()
    {
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(93451340);
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 1, 01, 17, 23, 00));

        var todaysResult = await _sut.GetReferenceNumber();

        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2125, 1, 01, 17, 23, 00));
        var hundredYearsResult = await _sut.GetReferenceNumber();

        hundredYearsResult.Should().Be(todaysResult);
    }

    [Fact(Skip =
        "Very slow test! Can change the value of SequenceMax to something smaller if you want to see a full run")]
    // [Fact]
    public async Task GetReferenceNumber_Unique_References_Within_Partition()
    {
        var generatedDigitReferences = new HashSet<long>();

        _timeProvider.Setup(x => x.GetUtcNow()).Returns(DateTimeOffset.UtcNow.AddDays(67));

        for (var i = 0; i < Provider.SequenceMax; i++)
        {
            _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(i);
            var referenceNumber = await _sut.GetReferenceNumber();

            var digitReference = long.Parse(referenceNumber.Replace("-", string.Empty));

            generatedDigitReferences.Add(digitReference);
        }

        //if the distinct list of generatedReferences is equal to its own count, that means it is a unique list
        Assert.Equal(generatedDigitReferences.Distinct().Count(), generatedDigitReferences.Count);

        // every value 0,...,N-1 was hit exactly once
        Assert.Equal(Provider.SequenceMax, generatedDigitReferences.Count);

        //all generated references are valid
        Assert.True(generatedDigitReferences.All(digitReference =>
            _sut.IsValidBookingReference(
                Provider.FormatBookingReference(digitReference.ToString()))));
    }
}
