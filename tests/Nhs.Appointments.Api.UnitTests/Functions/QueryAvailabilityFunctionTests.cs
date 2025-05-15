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
using System.Text;
#pragma warning disable CS0618 // Keep availabilityCalculator around until MultipleServicesEnabled is stable

namespace Nhs.Appointments.Api.Tests.Functions;

public class QueryAvailabilityFunctionTests
{
    private static readonly DateOnly Date = new DateOnly(2077, 1, 1);
    private readonly Mock<IAvailabilityCalculator> _availabilityCalculator = new();
    private readonly Mock<IAllocationStateService> _allocationStateService = new();
    private readonly Mock<IAvailabilityGrouper> _availabilityGrouper = new();
    private readonly Mock<IAvailabilityGrouperFactory> _availabilityGrouperFactory = new();
    private readonly Mock<ILogger<QueryAvailabilityFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly QueryAvailabilityFunction _sut;
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IValidator<QueryAvailabilityRequest>> _validator = new();
    private readonly Mock<IHasConsecutiveCapacityFilter> _hasConsecutiveCapacityFilter = new();
    private readonly Mock<IFeatureToggleHelper> _featureToggleHelper = new();

    public QueryAvailabilityFunctionTests()
    {
        _sut = new QueryAvailabilityFunction(
            _availabilityCalculator.Object,
            _allocationStateService.Object,
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

        var request = new QueryAvailabilityRequest(
            ["2de5bb57-060f-4cb5-b14d-16587d0c2e8f", "34e990af-5dc9-43a6-8895-b9123216d699"],
            "COVID",
            new DateOnly(2077, 01, 01),
            new DateOnly(2077, 01, 01),
            QueryType.Days,
            null);

        var httpRequest = CreateRequest(request);

        if (await _sut.RunAsync(httpRequest) is ContentResult result)
        {
            result.StatusCode.Should().Be(200);
            _hasConsecutiveCapacityFilter.Verify(
                x => x.SessionHasConsecutiveSessions(It.IsAny<IEnumerable<SessionInstance>>(), It.IsAny<int>()),
                Times.Never);
            var response = await ReadResponseAsync<QueryAvailabilityResponse>(result.Content);
            response.Count.Should().Be(2);
            response[0].site.Should().Be("2de5bb57-060f-4cb5-b14d-16587d0c2e8f");
            response[1].site.Should().Be("34e990af-5dc9-43a6-8895-b9123216d699");
        }
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
            ["2de5bb57-060f-4cb5-b14d-16587d0c2e8f"],
            "COVID",
            new DateOnly(2077, 01, 01),
            new DateOnly(2077, 01, 01),
            queryType,
            null);

        var httpRequest = CreateRequest(request);

        await _sut.RunAsync(httpRequest);
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
            ["2de5bb57-060f-4cb5-b14d-16587d0c2e8f"],
            "COVID",
            new DateOnly(2077, 01, 01),
            new DateOnly(2077, 01, 03),
            QueryType.Days,
            null);

        var httpRequest = CreateRequest(request);

        if (await _sut.RunAsync(httpRequest) is ContentResult result)
        {
            result.StatusCode.Should().Be(200);
            _hasConsecutiveCapacityFilter.Verify(
                x => x.SessionHasConsecutiveSessions(It.IsAny<IEnumerable<SessionInstance>>(), It.IsAny<int>()),
                Times.Never);
            var response = await ReadResponseAsync<QueryAvailabilityResponse>(result.Content);

            response[0].availability[0].date.Should().Be(new DateOnly(2077, 01, 01));
            response[0].availability[1].date.Should().Be(new DateOnly(2077, 01, 02));
            response[0].availability[2].date.Should().Be(new DateOnly(2077, 01, 03));
        }
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
            ["2de5bb57-060f-4cb5-b14d-16587d0c2e8f"],
            "COVID",
            new DateOnly(2077, 01, 01),
            new DateOnly(2077, 01, 03),
            QueryType.Days,
            2);

        var httpRequest = CreateRequest(request);

        if (await _sut.RunAsync(httpRequest) is ContentResult result)
        {
            result.StatusCode.Should().Be(200);
        }

        _hasConsecutiveCapacityFilter.Verify(x => x.SessionHasConsecutiveSessions(It.IsAny<IEnumerable<SessionInstance>>(), It.IsAny<int>()), Times.Once);
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
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(It.Is<string>(p => p == "JointBookings"))).ReturnsAsync(false);
        _hasConsecutiveCapacityFilter.Setup(x => x.SessionHasConsecutiveSessions(It.IsAny<IEnumerable<SessionInstance>>(), It.IsAny<int>())).Returns(blocks.AsEnumerable());

        var request = new QueryAvailabilityRequest(
            ["2de5bb57-060f-4cb5-b14d-16587d0c2e8f"],
            "COVID",
            new DateOnly(2077, 01, 01),
            new DateOnly(2077, 01, 03),
            QueryType.Days, 
            null);

        var httpRequest = CreateRequest(request);

        await _sut.RunAsync(httpRequest);

        _availabilityGrouper.Verify(x => x.GroupAvailability(It.IsAny<IEnumerable<SessionInstance>>()),
            Times.Exactly(3));
        _hasConsecutiveCapacityFilter.Verify(x => x.SessionHasConsecutiveSessions(It.IsAny<IEnumerable<SessionInstance>>(), It.IsAny<int>()), Times.Never);
    }
    
    [Fact]
    public async Task RunAsync_CallsAvailabilityCalculator_WhenMultipleServicesDisabled()
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
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(It.Is<string>(p => p == Flags.MultipleServices))).ReturnsAsync(false);
        
        var request = new QueryAvailabilityRequest(
            ["2de5bb57-060f-4cb5-b14d-16587d0c2e8f"],
            "COVID",
            new DateOnly(2077, 01, 01),
            new DateOnly(2077, 01, 03),
            QueryType.Days, 
            null);

        var httpRequest = CreateRequest(request);

        await _sut.RunAsync(httpRequest);
        
        _availabilityCalculator.Verify(x => x.CalculateAvailability("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", "COVID", new DateOnly(2077, 01, 01),
                new DateOnly(2077, 01, 03)),
            Times.Exactly(1));
        _allocationStateService.Verify(x => x.BuildAllocation("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", new DateTime(2077, 01, 01, 0, 0, 0),
                new DateTime(2077, 01, 03, 23, 59, 59, 59)),
            Times.Exactly(0));
    }
    
    [Fact]
    public async Task RunAsync_CallsAllocationStateService_WhenMultipleServicesEnabled()
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
        _allocationStateService.Setup(x =>
                x.BuildAllocation(It.IsAny<string>(), It.IsAny<DateTime>(),
                    It.IsAny<DateTime>()))
            .ReturnsAsync(new AllocationState()
            {
                AvailableSlots = blocks.ToList()
            });
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(It.Is<string>(p => p == Flags.MultipleServices))).ReturnsAsync(true);
        
        var request = new QueryAvailabilityRequest(
            ["2de5bb57-060f-4cb5-b14d-16587d0c2e8f"],
            "COVID",
            new DateOnly(2077, 01, 01),
            new DateOnly(2077, 01, 03),
            QueryType.Days, 
            null);

        var httpRequest = CreateRequest(request);

        await _sut.RunAsync(httpRequest);
        
        _availabilityCalculator.Verify(x => x.CalculateAvailability("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", "COVID", new DateOnly(2077, 01, 01),
                new DateOnly(2077, 01, 03)),
            Times.Exactly(0));
        _allocationStateService.Verify(x => x.BuildAllocation("2de5bb57-060f-4cb5-b14d-16587d0c2e8f", new DateTime(2077, 01, 01, 0, 0, 0),
                new DateTime(2077, 01, 03, 23, 59, 59)),
            Times.Exactly(1));
    }

    private static HttpRequest CreateRequest(QueryAvailabilityRequest request)
    {
        return CreateRequest(request.Sites, request.From, request.Until, request.Service, request.QueryType, request.Consecutive);
    }

    private static HttpRequest CreateRequest(string[] sites, DateOnly from, DateOnly until, string service,
        QueryType queryType, int? consecutive)
    {
        var sitesArray = string.Join(",", sites.Select(x => $"\"{x}\""));

        var context = new DefaultHttpContext();
        var request = context.Request;
        var body = consecutive.HasValue ? $"{{ \"sites\": [{sitesArray}], \"service\": \"{service}\", \"from\":  \"{from.ToString(DateTimeFormats.DateOnly)}\", \"until\": \"{until.ToString(DateTimeFormats.DateOnly)}\", \"queryType\": \"{queryType}\", \"consecutive\": {consecutive} }} " : $"{{ \"sites\": [{sitesArray}], \"service\": \"{service}\", \"from\":  \"{from.ToString(DateTimeFormats.DateOnly)}\", \"until\": \"{until.ToString(DateTimeFormats.DateOnly)}\", \"queryType\": \"{queryType}\" }} ";
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
