using System.Text;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.UnitTests.AvailabilityCalculations;

namespace Nhs.Appointments.Api.Tests.Functions;

public class QueryAvailabilityFunctionTests : AvailabilityCalculationsBase
{
    private static readonly DateOnly Date = new DateOnly(2077, 1, 1);
    private readonly Mock<IAvailabilityGrouper> _availabilityGrouper = new();
    private readonly Mock<IAvailabilityCalculator> _availabilityCalculator = new();
    private readonly Mock<IAvailabilityGrouperFactory> _availabilityGrouperFactory = new();
    private readonly Mock<ILogger<QueryAvailabilityFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly QueryAvailabilityFunction _query_sut;
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IFeatureToggleHelper> _featureToggleHelper = new();
    private readonly Mock<IHasConsecutiveCapacityFilter> _hasConsecutiveCapacityFilter = new();
    private readonly Mock<IValidator<QueryAvailabilityRequest>> _validator = new();

    public QueryAvailabilityFunctionTests()
    {
        _query_sut = new QueryAvailabilityFunction(
            _availabilityCalculator.Object,
            base._sut,
            _validator.Object,
            _availabilityGrouperFactory.Object,
            _userContextProvider.Object,
            _logger.Object,
            _metricsRecorder.Object,
            _featureToggleHelper.Object,
            _hasConsecutiveCapacityFilter.Object);

        _availabilityGrouperFactory.Setup(x => x.Create(It.IsAny<QueryType>()))
            .Returns(_availabilityGrouper.Object);
        
        _validator.Setup(x => x.ValidateAsync(It.IsAny<QueryAvailabilityRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task RunAsync_ReturnsSuccessResponse_WhenMultipleSitesAreQueried()
    {
        var slots = AvailabilityHelper.CreateTestSlots(Date, new TimeOnly(9, 0), new TimeOnly(10, 0),
            TimeSpan.FromMinutes(5));
        var responseBlocks = CreateAmPmResponseBlocks(12, 0);

        _availabilityGrouper.Setup(x => x.GroupAvailability(It.IsAny<IEnumerable<SessionInstance>>()))
            .Returns(responseBlocks);

        _availabilityCalculator.Setup(x =>
            x.CalculateAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>()))
            .ReturnsAsync(slots.AsEnumerable());
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(It.Is<string>(p => p == "JointBookings"))).ReturnsAsync(false);
        _hasConsecutiveCapacityFilter.Setup(x => x.SessionHasConsecutiveSessions(It.IsAny<IEnumerable<SessionInstance>>(), It.IsAny<int>())).Returns(slots.AsEnumerable());

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Purple"], capacity: 1),
            TestSession("09:00", "09:10", ["Blue", "Purple"], capacity: 1),
            TestSession("09:00", "09:10", ["Blue", "Purple"], capacity: 1),
            TestSession("09:00", "09:10", ["Blue", "Green"], capacity: 1),
            TestSession("09:00", "09:10", ["Green", "Blue"], capacity: 1),
            TestSession("09:00", "09:10", ["Purple", "Green"], capacity: 1),
        };
        
        SetupAvailabilityAndBookings([], sessions);
        
        var request = new QueryAvailabilityRequest(
            new[] { "2de5bb57-060f-4cb5-b14d-16587d0c2e8f", "34e990af-5dc9-43a6-8895-b9123216d699" },
            "COVID",
            new DateOnly(2077, 01, 01),
            new DateOnly(2077, 01, 01),
            QueryType.Days,
            null);

        var httpRequest = CreateRequest(request);

        var result = await _query_sut.RunAsync(httpRequest) as ContentResult;
        result.StatusCode.Should().Be(200);
        _hasConsecutiveCapacityFilter.Verify(x => x.SessionHasConsecutiveSessions(It.IsAny<IEnumerable<SessionInstance>>(), It.IsAny<int>()), Times.Never);
        var response = await ReadResponseAsync<QueryAvailabilityResponse>(result.Content);
        response.Count.Should().Be(2);
        response[0].site.Should().Be("2de5bb57-060f-4cb5-b14d-16587d0c2e8f");
        response[1].site.Should().Be("34e990af-5dc9-43a6-8895-b9123216d699");
    }

    [Theory]
    [InlineData(QueryType.Days)]
    [InlineData(QueryType.Hours)]
    [InlineData(QueryType.Slots)]
    public async Task RunAsync_ReturnsCorrectAvailabilityGrouper_WhenCalledWithConfiguredQueryType(QueryType queryType)
    {
        var slots = AvailabilityHelper.CreateTestSlots(Date, new TimeOnly(9, 0), new TimeOnly(10, 0),
            TimeSpan.FromMinutes(5));
        var responseBlocks = CreateAmPmResponseBlocks(12, 0);

        _availabilityGrouper.Setup(x => x.GroupAvailability(It.IsAny<IEnumerable<SessionInstance>>()))
            .Returns(responseBlocks);
        _availabilityCalculator.Setup(x =>
                x.CalculateAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(),
                    It.IsAny<DateOnly>()))
            .ReturnsAsync(slots.AsEnumerable());
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(It.Is<string>(p => p == "JointBookings"))).ReturnsAsync(false);
        _hasConsecutiveCapacityFilter.Setup(x => x.SessionHasConsecutiveSessions(It.IsAny<IEnumerable<SessionInstance>>(), It.IsAny<int>())).Returns(slots.AsEnumerable());

        var request = new QueryAvailabilityRequest(
            new[] { "2de5bb57-060f-4cb5-b14d-16587d0c2e8f" },
            "COVID",
            new DateOnly(2077, 01, 01),
            new DateOnly(2077, 01, 01),
            queryType,
            null);

        var httpRequest = CreateRequest(request);

        await _query_sut.RunAsync(httpRequest);
        _availabilityGrouperFactory.Verify(x => x.Create(queryType), Times.Once());
    }

    [Fact]
    public async Task RunAsync_ReturnsResults_ForEachDayInRequest()
    {
        var slots = AvailabilityHelper.CreateTestSlots(Date, new TimeOnly(9, 0), new TimeOnly(10, 0),
            TimeSpan.FromMinutes(5));
        var responseBlocks = CreateAmPmResponseBlocks(12, 0);

        _availabilityGrouper.Setup(x => x.GroupAvailability(It.IsAny<IEnumerable<SessionInstance>>()))
            .Returns(responseBlocks);
        _availabilityCalculator.Setup(x =>
                x.CalculateAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(),
                    It.IsAny<DateOnly>()))
            .ReturnsAsync(slots.AsEnumerable());
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(It.Is<string>(p => p == "JointBookings"))).ReturnsAsync(false);
        _hasConsecutiveCapacityFilter.Setup(x => x.SessionHasConsecutiveSessions(It.IsAny<IEnumerable<SessionInstance>>(), It.IsAny<int>())).Returns(slots.AsEnumerable());

        var request = new QueryAvailabilityRequest(
            new[] { "2de5bb57-060f-4cb5-b14d-16587d0c2e8f" },
            "COVID",
            new DateOnly(2077, 01, 01),
            new DateOnly(2077, 01, 03),
            QueryType.Days,
            null);

        var httpRequest = CreateRequest(request);

        var result = await _query_sut.RunAsync(httpRequest) as ContentResult;
        result.StatusCode.Should().Be(200);
        var response = await ReadResponseAsync<QueryAvailabilityResponse>(result.Content);

        response[0].availability[0].date.Should().Be(new DateOnly(2077, 01, 01));
        response[0].availability[1].date.Should().Be(new DateOnly(2077, 01, 02));
        response[0].availability[2].date.Should().Be(new DateOnly(2077, 01, 03));
    }
    
    [Fact]
    public async Task RunAsync_RunsJointBookings_WhenJointBookingsEnabled()
    {
        var slots = AvailabilityHelper.CreateTestSlots(Date, new TimeOnly(9, 0), new TimeOnly(10, 0),
            TimeSpan.FromMinutes(5));
        var responseBlocks = CreateAmPmResponseBlocks(12, 0);

        _availabilityGrouper.Setup(x => x.GroupAvailability(It.IsAny<IEnumerable<SessionInstance>>()))
            .Returns(responseBlocks);
        _availabilityCalculator.Setup(x =>
                x.CalculateAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(),
                    It.IsAny<DateOnly>()))
            .ReturnsAsync(slots.AsEnumerable());
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(It.Is<string>(p => p == "JointBookings"))).ReturnsAsync(true);
        _hasConsecutiveCapacityFilter.Setup(x => x.SessionHasConsecutiveSessions(It.IsAny<IEnumerable<SessionInstance>>(), It.IsAny<int>())).Returns(slots.AsEnumerable());

        var request = new QueryAvailabilityRequest(
            new[] { "2de5bb57-060f-4cb5-b14d-16587d0c2e8f" },
            "COVID",
            new DateOnly(2077, 01, 01),
            new DateOnly(2077, 01, 03),
            QueryType.Days,
            2);

        var httpRequest = CreateRequest(request);

        var result = await _query_sut.RunAsync(httpRequest) as ContentResult;
        result.StatusCode.Should().Be(200);
        _hasConsecutiveCapacityFilter.Verify(x => x.SessionHasConsecutiveSessions(It.IsAny<IEnumerable<SessionInstance>>(), It.IsAny<int>()), Times.Once);
    }
    
    
    [Theory(Skip = "Turn on when feature management mocked")]
    [InlineData("Purple")]
    [InlineData("Green")]
    [InlineData("Blue")]
    public async Task RunAsync_BestFitModel_QuerySlots(string queriedService)
    {
        _availabilityGrouperFactory.Setup(x => x.Create(QueryType.Slots))
            .Returns(new SlotAvailabilityGrouper());
        
        var bookings = new List<Booking>
        {
            TestBooking("1", "Purple", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "Green", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "Blue", avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "Purple", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "Green", avStatus: "Orphaned", creationOrder: 5)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Purple"], capacity: 1),
            TestSession("09:00", "09:10", ["Blue", "Purple"], capacity: 1),
            TestSession("09:00", "09:10", ["Blue", "Purple"], capacity: 1),
            TestSession("09:00", "09:10", ["Blue", "Green"], capacity: 1),
            TestSession("09:00", "09:10", ["Green", "Blue"], capacity: 1),
            TestSession("09:00", "09:10", ["Purple", "Green"], capacity: 1),
        };
        
        SetupAvailabilityAndBookings(bookings, sessions);
        
        var request = new QueryAvailabilityRequest(
            new[] { MockSite },
            queriedService,
            new DateOnly(2025, 01, 01),
            new DateOnly(2025, 01, 01),
            QueryType.Slots, 
            null);

        var httpRequest = CreateRequest(request);

        var result = await _query_sut.RunAsync(httpRequest) as ContentResult;
        result.StatusCode.Should().Be(200);
        var response = await ReadResponseAsync<QueryAvailabilityResponse>(result.Content);

        response[0].availability[0].date.Should().Be(new DateOnly(2025, 01, 01));
        var expectedBlocks = new QueryAvailabilityResponseBlock(new TimeOnly(09, 00, 00), new TimeOnly(09, 10, 00), 1);
        response[0].availability[0].blocks.Should().BeEquivalentTo([expectedBlocks]);
    }
    
    [Theory(Skip = "Turn on when feature management mocked")]
    [InlineData("Purple")]
    [InlineData("Green")]
    [InlineData("Blue")]
    public async Task RunAsync_BestFitModel_QuerySlots_2(string queriedService)
    {
        _availabilityGrouperFactory.Setup(x => x.Create(QueryType.Slots))
            .Returns(new SlotAvailabilityGrouper());
        
        var bookings = new List<Booking>
        {
            TestBooking("1", "Purple", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "Green", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "Blue", avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "Purple", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "Green", avStatus: "Orphaned", creationOrder: 5)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "10:00", ["Green", "Purple"], capacity: 1),
            TestSession("09:00", "10:00", ["Blue", "Purple"], capacity: 1),
            TestSession("09:00", "10:00", ["Blue", "Purple"], capacity: 1),
            TestSession("09:00", "10:00", ["Blue", "Green"], capacity: 1),
            TestSession("09:00", "10:00", ["Green", "Blue"], capacity: 1),
            TestSession("09:00", "10:00", ["Purple", "Green"], capacity: 1),
        };
        
        SetupAvailabilityAndBookings(bookings, sessions);
        
        var request = new QueryAvailabilityRequest(
            new[] { MockSite },
            queriedService,
            new DateOnly(2025, 01, 01),
            new DateOnly(2025, 01, 01),
            QueryType.Slots,
            null);

        var httpRequest = CreateRequest(request);

        var result = await _query_sut.RunAsync(httpRequest) as ContentResult;
        result.StatusCode.Should().Be(200);
        var response = await ReadResponseAsync<QueryAvailabilityResponse>(result.Content);

        response[0].availability[0].date.Should().Be(new DateOnly(2025, 01, 01));
        var expectedBlock1 = new QueryAvailabilityResponseBlock(new TimeOnly(09, 00, 00), new TimeOnly(09, 10, 00), 1);
        var expectedBlock2 = new QueryAvailabilityResponseBlock(new TimeOnly(09, 10, 00), new TimeOnly(09, 20, 00), 4);
        var expectedBlock3 = new QueryAvailabilityResponseBlock(new TimeOnly(09, 20, 00), new TimeOnly(09, 30, 00), 4);
        var expectedBlock4 = new QueryAvailabilityResponseBlock(new TimeOnly(09, 30, 00), new TimeOnly(09, 40, 00), 4);
        var expectedBlock5 = new QueryAvailabilityResponseBlock(new TimeOnly(09, 40, 00), new TimeOnly(09, 50, 00), 4);
        var expectedBlock6 = new QueryAvailabilityResponseBlock(new TimeOnly(09, 50, 00), new TimeOnly(10, 00, 00), 4);
        
        response[0].availability[0].blocks.Should().BeEquivalentTo([expectedBlock1, expectedBlock2, expectedBlock3, expectedBlock4, expectedBlock5, expectedBlock6]);
    }

    
    /// <summary>
    /// Prove that earlier bookings take the potential slots as needed, meaning no room for purple.
    /// </summary>
    [Fact(Skip = "Turn on when feature management mocked")]
    public async Task RunAsync_BestFitModel_QuerySlots_3()
    {
        _availabilityGrouperFactory.Setup(x => x.Create(QueryType.Slots))
            .Returns(new SlotAvailabilityGrouper());
        
        var bookings = new List<Booking>
        {
            TestBooking("1", "Purple", avStatus: "Supported", creationOrder: 1),
            TestBooking("2", "Green", avStatus: "Supported", creationOrder: 2),
            TestBooking("3", "Blue", avStatus: "Supported", creationOrder: 3),
            TestBooking("4", "Purple", avStatus: "Supported", creationOrder: 4),
            TestBooking("5", "Green", avStatus: "Supported", creationOrder: 5)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Purple"], capacity: 1),
            TestSession("09:00", "09:10", ["Blue", "Purple"], capacity: 1),
            TestSession("09:00", "09:10", ["Blue", "Purple"], capacity: 1),
            TestSession("09:00", "09:10", ["Orange", "Grey"], capacity: 1),
            TestSession("09:00", "09:10", ["Orange", "Purple"], capacity: 1),
            TestSession("09:00", "09:10", ["Grey", "Green"], capacity: 1),
        };
        
        SetupAvailabilityAndBookings(bookings, sessions);
        
        var request = new QueryAvailabilityRequest(
            new[] { MockSite },
            "Purple",
            new DateOnly(2025, 01, 01),
            new DateOnly(2025, 01, 01),
            QueryType.Slots,
            null);

        var httpRequest = CreateRequest(request);

        var result = await _query_sut.RunAsync(httpRequest) as ContentResult;
        result.StatusCode.Should().Be(200);
        var response = await ReadResponseAsync<QueryAvailabilityResponse>(result.Content);

        response[0].availability[0].blocks.Should().BeEmpty();
    }
    
    /// <summary>
    /// Prove that no availability for purple if no sessions have purple.
    /// </summary>
    [Fact(Skip = "Turn on when feature management mocked")]
    public async Task RunAsync_BestFitModel_QuerySlots_4()
    {
        _availabilityGrouperFactory.Setup(x => x.Create(QueryType.Slots))
            .Returns(new SlotAvailabilityGrouper());
        
        var bookings = new List<Booking>
        {
            TestBooking("1", "Orange", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "Green", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "Blue", avStatus: "Orphaned", creationOrder: 3),
            TestBooking("4", "Blue", avStatus: "Orphaned", creationOrder: 4),
            TestBooking("5", "Green", avStatus: "Orphaned", creationOrder: 5)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Orange"], capacity: 1),
            TestSession("09:00", "09:10", ["Blue", "Green"], capacity: 1),
            TestSession("09:00", "09:10", ["Blue", "Black"], capacity: 1),
            TestSession("09:00", "09:10", ["Orange", "Grey"], capacity: 1),
            TestSession("09:00", "09:10", ["Orange", "Black"], capacity: 1),
            TestSession("09:00", "09:10", ["Grey", "Green"], capacity: 1),
        };
        
        SetupAvailabilityAndBookings(bookings, sessions);
        
        var request = new QueryAvailabilityRequest(
            new[] { MockSite },
            "Purple",
            new DateOnly(2025, 01, 01),
            new DateOnly(2025, 01, 01),
            QueryType.Slots,
            null);

        var httpRequest = CreateRequest(request);

        var result = await _query_sut.RunAsync(httpRequest) as ContentResult;
        result.StatusCode.Should().Be(200);
        var response = await ReadResponseAsync<QueryAvailabilityResponse>(result.Content);

        response[0].availability[0].blocks.Should().BeEmpty();
    }
    
    /// <summary>
    /// Prove that earlier bookings take the potential slots as needed, meaning no room for purple.
    /// </summary>
    [Theory(Skip = "Turn on when feature management mocked")]
    [InlineData("Purple")]
    [InlineData("Green")]
    [InlineData("Blue")]
    [InlineData("Red")]
    [InlineData("Orange")]
    public async Task RunAsync_BestFitModel_QuerySlots_5(string queriedService)
    {
        _availabilityGrouperFactory.Setup(x => x.Create(QueryType.Slots))
            .Returns(new SlotAvailabilityGrouper());
        
        var bookings = new List<Booking>
        {
            TestBooking("1", "Orange", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "Blue", avStatus: "Orphaned", creationOrder: 2),
            TestBooking("3", "Green", avStatus: "Orphaned", creationOrder: 3)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Purple"], capacity: 1),
            TestSession("09:00", "09:10", ["Orange", "Red"], capacity: 1),
            TestSession("09:00", "09:10", ["Blue", "Orange"], capacity: 1),
            TestSession("09:00", "09:10", ["Green", "Blue"], capacity: 1)
        };
        
        SetupAvailabilityAndBookings(bookings, sessions);
        
        var request = new QueryAvailabilityRequest(
            new[] { MockSite },
            queriedService,
            new DateOnly(2025, 01, 01),
            new DateOnly(2025, 01, 01),
            QueryType.Slots,
            null);

        var httpRequest = CreateRequest(request);

        var result = await _query_sut.RunAsync(httpRequest) as ContentResult;
        result.StatusCode.Should().Be(200);
        var response = await ReadResponseAsync<QueryAvailabilityResponse>(result.Content);

        response[0].availability[0].date.Should().Be(new DateOnly(2025, 01, 01));
        var expectedBlocks = new QueryAvailabilityResponseBlock(new TimeOnly(09, 00, 00), new TimeOnly(09, 10, 00), 1);
        response[0].availability[0].blocks.Should().BeEquivalentTo([expectedBlocks]);
    }
    
    [Theory(Skip = "Turn on when feature management mocked")]
    [InlineData("Purple", 1)]
    [InlineData("Green", 2)]
    [InlineData("Blue", 1)]
    [InlineData("Red", 1)]
    [InlineData("Orange", 1)]
    public async Task RunAsync_BestFitModel_QuerySlots_6(string queriedService, int expectedCapacity)
    {
        _availabilityGrouperFactory.Setup(x => x.Create(QueryType.Slots))
            .Returns(new SlotAvailabilityGrouper());
        
        var bookings = new List<Booking>
        {
            TestBooking("1", "Orange", avStatus: "Orphaned", creationOrder: 1),
            TestBooking("2", "Blue", avStatus: "Orphaned", creationOrder: 2)
        };

        var sessions = new List<SessionInstance>
        {
            TestSession("09:00", "09:10", ["Green", "Purple"], capacity: 1),
            TestSession("09:00", "09:10", ["Orange", "Red"], capacity: 1),
            TestSession("09:00", "09:10", ["Blue", "Orange"], capacity: 1),
            TestSession("09:00", "09:10", ["Green", "Blue"], capacity: 1)
        };
        
        SetupAvailabilityAndBookings(bookings, sessions);
        
        var request = new QueryAvailabilityRequest(
            new[] { MockSite },
            queriedService,
            new DateOnly(2025, 01, 01),
            new DateOnly(2025, 01, 01),
            QueryType.Slots,
            null);

        var httpRequest = CreateRequest(request);

        var result = await _query_sut.RunAsync(httpRequest) as ContentResult;
        result.StatusCode.Should().Be(200);
        var response = await ReadResponseAsync<QueryAvailabilityResponse>(result.Content);

        response[0].availability[0].date.Should().Be(new DateOnly(2025, 01, 01));
        var expectedBlocks = new QueryAvailabilityResponseBlock(new TimeOnly(09, 00, 00), new TimeOnly(09, 10, 00), expectedCapacity);
        response[0].availability[0].blocks.Should().BeEquivalentTo([expectedBlocks]);
    }
    
    [Fact]
    public async Task RunAsync_CallsGrouperWithCorrectSlots_ForEachDayInRequest()
    {
        var blocks = new[]
        {
            new SessionInstance(new DateTime(2077, 1, 1, 9, 0, 0), new DateTime(2077, 1, 1, 9, 5, 0)),
            new SessionInstance(new DateTime(2077, 1, 2, 10, 0, 0), new DateTime(2077, 1, 2, 10, 5, 0)),
            new SessionInstance(new DateTime(2077, 1, 3, 11, 0, 0), new DateTime(2077, 1, 3, 11, 5, 0)),
        };
        var responseBlocks = CreateAmPmResponseBlocks(12, 0);

        _availabilityGrouper.Setup(x => x.GroupAvailability(It.IsAny<IEnumerable<SessionInstance>>()))
            .Returns(responseBlocks);
        _availabilityCalculator.Setup(x =>
                x.CalculateAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(),
                    It.IsAny<DateOnly>()))
            .ReturnsAsync(blocks.AsEnumerable());

        var request = new QueryAvailabilityRequest(
            new[] { "2de5bb57-060f-4cb5-b14d-16587d0c2e8f" },
            "COVID",
            new DateOnly(2077, 01, 01),
            new DateOnly(2077, 01, 03),
            QueryType.Days,
            null);

        var httpRequest = CreateRequest(request);

        await _query_sut.RunAsync(httpRequest);

        _availabilityGrouper.Verify(x => x.GroupAvailability(It.IsAny<IEnumerable<SessionInstance>>()),
            Times.Exactly(3));
    }

    private static HttpRequest CreateRequest(QueryAvailabilityRequest request)
    {
        return CreateRequest(request.Sites, request.From, request.Until, request.Service, request.QueryType);
    }

    private static HttpRequest CreateRequest(string[] sites, DateOnly from, DateOnly until, string service,
        QueryType queryType)
    {
        var sitesArray = string.Join(",", sites.Select(x => $"\"{x}\""));

        var context = new DefaultHttpContext();
        var request = context.Request;
        var body =
            $"{{ sites:[{sitesArray}], \"service\": \"{service}\", \"from\":  \"{from.ToString(DateTimeFormats.DateOnly)}\", \"until\": \"{until.ToString(DateTimeFormats.DateOnly)}\", \"queryType\": \"{queryType}\" }} ";
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        request.Headers.Append("Authorization", "Test 123");
        return request;
    }

    private static IEnumerable<QueryAvailabilityResponseBlock> CreateAmPmResponseBlocks(int amCount, int pmCount)
    {
        return new List<QueryAvailabilityResponseBlock>
        {
            new(new TimeOnly(0, 0), new TimeOnly(11, 59), amCount),
            new(new TimeOnly(12, 0), new TimeOnly(23, 59), pmCount),
        };
    }

    private static async Task<TRequest> ReadResponseAsync<TRequest>(string response)
    {
        var deserializerSettings = new JsonSerializerSettings
        {
            Converters =
            {
                new ShortTimeOnlyJsonConverter(),
                new ShortDateOnlyJsonConverter(),
                new NullableShortDateOnlyJsonConverter()
            },
        };
        var body = await new StringReader(response).ReadToEndAsync();
        return JsonConvert.DeserializeObject<TRequest>(body, deserializerSettings);
    }
}
