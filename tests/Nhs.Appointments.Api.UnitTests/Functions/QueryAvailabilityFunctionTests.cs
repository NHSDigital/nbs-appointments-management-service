using System.Text;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Json;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Functions;

public class QueryAvailabilityFunctionTests
{
    private static readonly DateOnly Date = new DateOnly(2077, 1, 1);
    private readonly QueryAvailabilityFunction _sut;
    private readonly Mock<IAvailabilityCalculator> _availabilityCalculator = new();
    private readonly Mock<IAvailabilityGrouperFactory> _availabilityGrouperFactory = new();
    private readonly Mock<IValidator<QueryAvailabilityRequest>> _validator = new();
    private readonly Mock<IAvailabilityGrouper> _availabilityGrouper = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<ILogger<QueryAvailabilityFunction>> _logger = new();

    public QueryAvailabilityFunctionTests()
    {
        _sut = new QueryAvailabilityFunction(
            _availabilityCalculator.Object, 
                _validator.Object, 
            _availabilityGrouperFactory.Object, 
            _userContextProvider.Object,
            _logger.Object);

        _availabilityGrouperFactory.Setup(x => x.Create(It.IsAny<QueryType>()))
            .Returns(_availabilityGrouper.Object);
        _validator.Setup(x => x.ValidateAsync(It.IsAny<QueryAvailabilityRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task RunAsync_ReturnsSuccessResponse_WhenMultipleSitesAreQueried()
    {
        var slots = AvailabilityHelper.CreateTestSlots(Date, new TimeOnly(9, 0), new TimeOnly(10, 0), TimeSpan.FromMinutes(5));
        var responseBlocks = CreateAmPmResponseBlocks(12, 0);

        _availabilityGrouper.Setup(x => x.GroupAvailability(It.IsAny<IEnumerable<SessionInstance>>()))
            .Returns(responseBlocks);
        _availabilityCalculator.Setup(x => x.CalculateAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(slots.AsEnumerable());
        
        var request = new QueryAvailabilityRequest(
            new[] { "1000", "1001" },
            "COVID", 
            "2077-01-01",
            "2077-01-01",
            QueryType.Days);

        var httpRequest = CreateRequest(request);
        
        var result = await _sut.RunAsync(httpRequest) as ContentResult;
        result.StatusCode.Should().Be(200);
        var response = ReadResponseAsync<QueryAvailabilityResponse>(result.Content);
        response.Result.Count.Should().Be(2);
        response.Result[0].site.Should().Be("1000");
        response.Result[1].site.Should().Be("1001");
    }      
    
    [Theory]
    [InlineData(QueryType.Days)]
    [InlineData(QueryType.Hours)]
    [InlineData(QueryType.Slots)]
    public async Task RunAsync_ReturnsCorrectAvailabilityGrouper_WhenCalledWithConfiguredQueryType(QueryType queryType)
    {
        var slots = AvailabilityHelper.CreateTestSlots(Date, new TimeOnly(9, 0), new TimeOnly(10, 0), TimeSpan.FromMinutes(5));
        var responseBlocks = CreateAmPmResponseBlocks(12, 0);

        _availabilityGrouper.Setup(x => x.GroupAvailability(It.IsAny<IEnumerable<SessionInstance>>()))
            .Returns(responseBlocks);
        _availabilityCalculator.Setup(x => x.CalculateAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(slots.AsEnumerable());
        
        var request = new QueryAvailabilityRequest(
            new[] { "1000" },
            "COVID", 
            "2077-01-01",
            "2077-01-01",
            queryType);

        var httpRequest = CreateRequest(request);

        await _sut.RunAsync(httpRequest);
        _availabilityGrouperFactory.Verify(x => x.Create(queryType), Times.Once());
    }

    [Fact]
    public async Task RunAsync_ReturnsResults_ForEachDayInRequest()
    {
        var slots = AvailabilityHelper.CreateTestSlots(Date, new TimeOnly(9, 0), new TimeOnly(10, 0), TimeSpan.FromMinutes(5));
        var responseBlocks = CreateAmPmResponseBlocks(12, 0);

        _availabilityGrouper.Setup(x => x.GroupAvailability(It.IsAny<IEnumerable<SessionInstance>>()))
            .Returns(responseBlocks);
        _availabilityCalculator.Setup(x => x.CalculateAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(slots.AsEnumerable());

        var request = new QueryAvailabilityRequest(
            new[] { "1000" },
            "COVID",
            "2077-01-01",
            "2077-01-03",
            QueryType.Days);

        var httpRequest = CreateRequest(request);

        var result = await _sut.RunAsync(httpRequest) as ContentResult;
        result.StatusCode.Should().Be(200);
        var response = ReadResponseAsync<QueryAvailabilityResponse>(result.Content);

        response.Result[0].availability[0].date.Should().Be(new DateOnly(2077, 01, 01));
        response.Result[0].availability[1].date.Should().Be(new DateOnly(2077, 01, 02));
        response.Result[0].availability[2].date.Should().Be(new DateOnly(2077, 01, 03));
    }

    [Fact]
    public async Task RunAsync_CallsGrouperWithCorrectSlots_ForEachDayInRequest()
    {
        var blocks = new[]
        {
            new SessionInstance(new DateTime(2077,1,1,9,0,0), new DateTime(2077,1,1,9,5,0)),
            new SessionInstance(new DateTime(2077,1,2,10,0,0), new DateTime(2077,1,2,10,5,0)),
            new SessionInstance(new DateTime(2077,1,3,11,0,0), new DateTime(2077,1,3,11,5,0)),
        };
        var responseBlocks = CreateAmPmResponseBlocks(12, 0);

        _availabilityGrouper.Setup(x => x.GroupAvailability(It.IsAny<IEnumerable<SessionInstance>>()))
            .Returns(responseBlocks);
        _availabilityCalculator.Setup(x => x.CalculateAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(blocks.AsEnumerable());

        var request = new QueryAvailabilityRequest(
            new[] { "1000" },
            "COVID",
            "2077-01-01",
            "2077-01-03",
            QueryType.Days);

        var httpRequest = CreateRequest(request);

        await _sut.RunAsync(httpRequest);
        
        _availabilityGrouper.Verify(x => x.GroupAvailability(It.IsAny<IEnumerable<SessionInstance>>()), Times.Exactly(3));        
    }

    private static HttpRequest CreateRequest(QueryAvailabilityRequest request)
    {
        return CreateRequest(request.Sites, request.From, request.Until, request.Service, request.QueryType);
    }
    
    private static HttpRequest CreateRequest(string[] sites, string from, string until, string service, QueryType queryType)
    {
        var sitesArray = String.Join(",", sites.Select(x => $"\"{x}\""));

        var context = new DefaultHttpContext();
        var request = context.Request;
        var body = $"{{ sites:[{sitesArray}], \"service\": \"{service}\", \"from\":  \"{from}\", \"until\": \"{until}\", \"queryType\": \"{queryType}\" }} ";
        request.Body =  new MemoryStream(Encoding.UTF8.GetBytes(body));
        request.Headers.Add("Authorization", "Test 123");
        return request;
    }       

    private static IEnumerable<QueryAvailabilityResponseBlock> CreateAmPmResponseBlocks(int amCount, int pmCount)
    {
        return new List<QueryAvailabilityResponseBlock>
        {
            new (new TimeOnly(0,0), new TimeOnly(11,59), amCount),
            new (new TimeOnly(12,0), new TimeOnly(23,59), pmCount),
        };
    }

    private static async Task<TRequest> ReadResponseAsync<TRequest>(string response)
    {
        var deserializerSettings = new JsonSerializerSettings
        {
            Converters =
            {
                new ShortTimeOnlyJsonConverter(), new ShortDateOnlyJsonConverter(), new NullableShortDateOnlyJsonConverter()
            },

        };
        var body = await new StringReader(response).ReadToEndAsync();
        return JsonConvert.DeserializeObject<TRequest>(body, deserializerSettings);
    }
}
