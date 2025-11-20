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
        _options.Setup(x => x.Value).Returns(new ReferenceNumberOptions { HmacKey = HmacTestSecretKey});
        _sut = new Provider(_options.Object, _bookingReferenceDocumentStore.Object, _logger.Object, _timeProvider.Object);
    }
    
    [Fact]
    public void IsValidBookingReference_Supports_Existing_V1_ReferenceFormats()
    {
        Assert.True(ProviderHelper.IsValidBookingReference("14-25-002341", _logger.Object));
        Assert.True(ProviderHelper.IsValidBookingReference("56-36-843502", _logger.Object));
        Assert.True(ProviderHelper.IsValidBookingReference("99-99-999999", _logger.Object));
        Assert.True(ProviderHelper.IsValidBookingReference("00-00-000000", _logger.Object));
        
        Assert.True(ProviderHelper.IsValidBookingReference("14-90-002341", _logger.Object));
        Assert.True(ProviderHelper.IsValidBookingReference("14-90-002342", _logger.Object));
        Assert.True(ProviderHelper.IsValidBookingReference("14-90-002343", _logger.Object));
        Assert.True(ProviderHelper.IsValidBookingReference("14-90-002344", _logger.Object));
        Assert.True(ProviderHelper.IsValidBookingReference("14-90-002345", _logger.Object));
        Assert.True(ProviderHelper.IsValidBookingReference("14-90-002346", _logger.Object));
        Assert.True(ProviderHelper.IsValidBookingReference("14-90-002347", _logger.Object));
        Assert.True(ProviderHelper.IsValidBookingReference("14-90-002348", _logger.Object));
        Assert.True(ProviderHelper.IsValidBookingReference("14-90-002349", _logger.Object));
        Assert.True(ProviderHelper.IsValidBookingReference("14-90-002340", _logger.Object));
        
        Assert.False(ProviderHelper.IsValidBookingReference("14-a0-002340", _logger.Object));
        Assert.False(ProviderHelper.IsValidBookingReference("1b-70-002340", _logger.Object));
        Assert.False(ProviderHelper.IsValidBookingReference("14-70-0023c0", _logger.Object));
        
        Assert.False(ProviderHelper.IsValidBookingReference("142-90-002340", _logger.Object));
        Assert.False(ProviderHelper.IsValidBookingReference("14-903-002340", _logger.Object));
        Assert.False(ProviderHelper.IsValidBookingReference("14-90-0023406", _logger.Object));
        Assert.False(ProviderHelper.IsValidBookingReference("1-90-002340", _logger.Object));
        Assert.False(ProviderHelper.IsValidBookingReference("14-9-002340", _logger.Object));
        Assert.False(ProviderHelper.IsValidBookingReference("14-90-00340", _logger.Object));
        
        Assert.False(ProviderHelper.IsValidBookingReference("1490002345", _logger.Object));
        Assert.False(ProviderHelper.IsValidBookingReference("149000234", _logger.Object));
        Assert.False(ProviderHelper.IsValidBookingReference("14900023454", _logger.Object));
        Assert.False(ProviderHelper.IsValidBookingReference("1490-002345", _logger.Object));
        Assert.False(ProviderHelper.IsValidBookingReference("14-90002345", _logger.Object));
        Assert.False(ProviderHelper.IsValidBookingReference(" 1490002345 ", _logger.Object));
        Assert.False(ProviderHelper.IsValidBookingReference(" 14-90-002345 ", _logger.Object));
        Assert.False(ProviderHelper.IsValidBookingReference("14-90-002345 ", _logger.Object));
        Assert.False(ProviderHelper.IsValidBookingReference(" 14-90-002345", _logger.Object));
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
    public async Task GetReferenceNumber_UsesAndSetsDatabaseMultiplierDerivation()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 5, 30, 9, 0, 59));
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(2345123);
        _bookingReferenceDocumentStore.Setup(x => x.TryGetPartitionKeySequenceMultiplier("3825")).ReturnsAsync((int?)null);
        _bookingReferenceDocumentStore.Setup(x => x.TryGetPartitionKeySequenceMultiplier("4625")).ReturnsAsync((int?)null);

        var result1 = await _sut.GetReferenceNumber();
        result1.Should().Be("3825-69268-6774");

        var expectedMultiplier1 = 28980599;
        
        //first time tryGet attempted
        _bookingReferenceDocumentStore.Verify(x => x.TryGetPartitionKeySequenceMultiplier("3825"), Times.Once);
        
        //first time calculated is stored
        _bookingReferenceDocumentStore.Verify(x => x.SetPartitionKeySequenceMultiplier("3825", expectedMultiplier1), Times.Once);
        _bookingReferenceDocumentStore.Verify(x => x.SetPartitionKeySequenceMultiplier("4625", expectedMultiplier1), Times.Never);
        
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 5, 31, 9, 0, 59));
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(2345124);
        _bookingReferenceDocumentStore.Setup(x => x.TryGetPartitionKeySequenceMultiplier("3825")).ReturnsAsync(expectedMultiplier1);
        
        _bookingReferenceDocumentStore.Invocations.Clear();
        
        var result2 = await _sut.GetReferenceNumber();
        result2.Should().Be("3825-98249-2768");
        
        //second time tryGet attempted
        _bookingReferenceDocumentStore.Verify(x => x.TryGetPartitionKeySequenceMultiplier("3825"), Times.Once);
        
        //second time calculated is not stored
        _bookingReferenceDocumentStore.Verify(x => x.SetPartitionKeySequenceMultiplier("3725", expectedMultiplier1), Times.Never);
        _bookingReferenceDocumentStore.Verify(x => x.SetPartitionKeySequenceMultiplier("3825", expectedMultiplier1), Times.Never);
        _bookingReferenceDocumentStore.Verify(x => x.SetPartitionKeySequenceMultiplier("3925", expectedMultiplier1), Times.Never);
        _bookingReferenceDocumentStore.Verify(x => x.SetPartitionKeySequenceMultiplier("4625", expectedMultiplier1), Times.Never);
        
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 6, 30, 9, 0, 59));
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(2345125);
        
        _bookingReferenceDocumentStore.Invocations.Clear();
        
        var result3 = await _sut.GetReferenceNumber();
        result3.Should().Be("4625-68731-6257");
        
        var expectedMultiplier2 = 84432373;
        
        _bookingReferenceDocumentStore.Verify(x => x.TryGetPartitionKeySequenceMultiplier("3825"), Times.Never);
        _bookingReferenceDocumentStore.Verify(x => x.TryGetPartitionKeySequenceMultiplier("4625"), Times.Once);
        
        //third time is stored as using a new partition key
        _bookingReferenceDocumentStore.Verify(x => x.SetPartitionKeySequenceMultiplier("3825", expectedMultiplier2), Times.Never);
        _bookingReferenceDocumentStore.Verify(x => x.SetPartitionKeySequenceMultiplier("4625", expectedMultiplier2), Times.Once);
    }
    
    /// <summary>
    /// If for an environment the HmacKey is updated, it MUST still use the previous multiplier for the same partition key
    /// It should only use the new HmacKey value when the partition moves along for the first time and then going forward
    /// This is to ensure we don't have collisions mid-partition with a HmacKey update!!
    /// </summary>
    [Fact]
    public async Task GetReferenceNumber_HmacKeyChange_UsesPreviousMultiplier_UntilPartitionChanges()
    {
        //initial HMAC key set
        _options.Setup(x => x.Value).Returns(new ReferenceNumberOptions { HmacKey = HmacTestSecretKey});
        
        _bookingReferenceDocumentStore.Setup(x => x.TryGetPartitionKeySequenceMultiplier("3825")).ReturnsAsync((int?)null);
        
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 5, 30, 9, 0, 59));
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(2345123);

        var result1 = await _sut.GetReferenceNumber();
        result1.Should().Be("3825-69268-6774");
        
        var initialHmacKeyPartition1ExpectedMultiplier = 28980599;
        
        //first time calculated is stored
        _bookingReferenceDocumentStore.Verify(x => x.SetPartitionKeySequenceMultiplier("3825", initialHmacKeyPartition1ExpectedMultiplier), Times.Once);
        
        //next booking generated in same partition key
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 5, 30, 10, 0, 59));
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(2345124);
        //the multiplier using HMAC Key 1 is stored for '3825'
        _bookingReferenceDocumentStore.Setup(x => x.TryGetPartitionKeySequenceMultiplier("3825")).ReturnsAsync(initialHmacKeyPartition1ExpectedMultiplier);
        
        byte[] updatedHmacKey =
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
        
        //AND the HMAC KEY is updated!!
        _options.Setup(x => x.Value).Returns(new ReferenceNumberOptions { HmacKey = updatedHmacKey});
        
        var updatedHmacKeyPartition1ExpectedMultiplier = 54654634;
        
        _bookingReferenceDocumentStore.Invocations.Clear();
        
        var result2 = await _sut.GetReferenceNumber();
        
        //this new value is derived using the initial HMAC key value since in same partition as existing record
        result2.Should().Be("3825-98249-2768");
        
        _bookingReferenceDocumentStore.Verify(x => x.TryGetPartitionKeySequenceMultiplier("3825"), Times.Once);
        _bookingReferenceDocumentStore.Verify(x => x.SetPartitionKeySequenceMultiplier("3825", initialHmacKeyPartition1ExpectedMultiplier), Times.Never);
        _bookingReferenceDocumentStore.Verify(x => x.SetPartitionKeySequenceMultiplier("3825", updatedHmacKeyPartition1ExpectedMultiplier), Times.Never);
        
        //verify new HMAC key used in derivation of new partition key
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 6, 3, 11, 0, 59));
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(2345125);
        
        //new partition key has no value as hasn't been generated for the first time yet
        _bookingReferenceDocumentStore.Setup(x => x.TryGetPartitionKeySequenceMultiplier("3925")).ReturnsAsync((int?)null);
        
        //both values are different as partition key has changed
        //prove it now uses the updated multiplier
        var initialHmacKeyPartition2ExpectedMultiplier = 7166467;
        var updatedHmacKeyPartition2ExpectedMultiplier = 45328053;
        
        _bookingReferenceDocumentStore.Invocations.Clear();
        
        var result3 = await _sut.GetReferenceNumber();
        
        //this has been generated using the updated hmac value
        
        //the expected result IF the key hadn't been updated
        result3.Should().NotBe("3925-60923-3759");
        
        //the actual expected result now the key has been updated
        result3.Should().Be("3925-50291-6252");
        
        //it tried to fetch an existing value
        _bookingReferenceDocumentStore.Verify(x => x.TryGetPartitionKeySequenceMultiplier("3925"), Times.Once);
        
        //prove only the new updated value is set
        _bookingReferenceDocumentStore.Verify(x => x.SetPartitionKeySequenceMultiplier("3925", initialHmacKeyPartition2ExpectedMultiplier), Times.Never);
        _bookingReferenceDocumentStore.Verify(x => x.SetPartitionKeySequenceMultiplier("3925", updatedHmacKeyPartition2ExpectedMultiplier), Times.Once);
        
        _bookingReferenceDocumentStore.Invocations.Clear();
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

            Assert.True(ProviderHelper.IsValidBookingReference(result, _logger.Object));
            result.Should().StartWith($"{dayStep:00}25");
        }
    }

    [Fact]
    public async Task GetReferenceNumber_GeneratesCorrectlyFormattedNumber_MaxDate_LeapYear()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2024, 12, 31, 23, 59, 59));
        _bookingReferenceDocumentStore.Setup(x => x.GetNextSequenceNumber()).ReturnsAsync(99999999);
        
        var result = await _sut.GetReferenceNumber();

        Assert.True(ProviderHelper.IsValidBookingReference(result, _logger.Object));
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
        Assert.True(ProviderHelper.IsValidBookingReference(result, _logger.Object));
        
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
        Assert.False(ProviderHelper.IsValidBookingReference("4016-90927-6171", _logger.Object));
        Assert.False(ProviderHelper.IsValidBookingReference("4016-90927-6172", _logger.Object));
        Assert.False(ProviderHelper.IsValidBookingReference("4016-90927-6173", _logger.Object));
        Assert.False(ProviderHelper.IsValidBookingReference("4016-90927-6175", _logger.Object));
        Assert.False(ProviderHelper.IsValidBookingReference("4016-90927-6176", _logger.Object));
        Assert.False(ProviderHelper.IsValidBookingReference("4016-90927-6177", _logger.Object));
        Assert.False(ProviderHelper.IsValidBookingReference("4016-90927-6178", _logger.Object));
        Assert.False(ProviderHelper.IsValidBookingReference("4016-90927-6179", _logger.Object));
        Assert.False(ProviderHelper.IsValidBookingReference("4016-90927-6170", _logger.Object));
        
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
    ///  If SequenceMax is ever changed from the original 100 million, it MUST be of the format 10^X to work with the current coprime sequence logic
    ///  i.e if number A ends in either a {1, 3, 7, 9}, then it is coprime with any number B of format: B = 10^X = (2^X)*(5^X) => all prime factors of B are 2 and 5
    /// </summary>
    [Fact]
    public void SequenceMax_Should_Be_Power_Of_Ten()
    {
        var log10 = Math.Log10(Provider.SequenceMax);
        
        //floating point check
        Math.Abs(log10 - Math.Round(log10)).Should().BeLessThan(1e-10);
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

        _options.Setup(x => x.Value).Returns(new ReferenceNumberOptions { HmacKey = hmacKey1});
        var multiplier1 = _sut.DeriveSequenceMultiplier(samePartitionKey);

        _options.Setup(x => x.Value).Returns(new ReferenceNumberOptions { HmacKey = hmacKey2});
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
        
        _options.Setup(x => x.Value).Returns(new ReferenceNumberOptions { HmacKey = hmacKey1});
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
        
        _options.Setup(x => x.Value).Returns(new ReferenceNumberOptions { HmacKey = hmacKey2});
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
            ProviderHelper.IsValidBookingReference(
                Provider.FormatBookingReference(digitReference.ToString()), _logger.Object)));
    }
}
