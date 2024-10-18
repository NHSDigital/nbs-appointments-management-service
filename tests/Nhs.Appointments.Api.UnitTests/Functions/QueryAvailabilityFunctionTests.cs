using System.Text;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
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

    /*[Fact]
    public async Task RunAsync_ReturnsSuccessResponse_WhenMultipleSitesAreQueried()
    {
        var blocks = AvailabilityHelper.CreateTestBlocks("10:00-11:00");
        var siteConfigurationA = CreateSiteConfiguration("1000", "COVID", "COVID");
        var siteConfigurationB = CreateSiteConfiguration("1001", "COVID", "COVID");
        var responseBlocks = CreateAmPmResponseBlocks(12, 0);

        _availabilityGrouper.Setup(x => x.GroupAvailability(It.IsAny<IEnumerable<TimePeriod>>(), It.IsAny<int>()))
            .Returns(responseBlocks);
        _availabilityCalculator.Setup(x => x.CalculateAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(blocks.AsEnumerable());
        _siteConfigurationService.Setup(x => x.GetSiteConfigurationAsync("1000")).ReturnsAsync(siteConfigurationA);
        _siteConfigurationService.Setup(x => x.GetSiteConfigurationAsync("1001")).ReturnsAsync(siteConfigurationB);
        
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
    }*/

    /*[Fact]
    public async Task RunAsync_ReturnsEmptyResponse_WhenServiceRequestedIsNotConfiguredForSite()
    {
        var blocks = AvailabilityHelper.CreateTestBlocks("10:00-11:00");
        var siteConfiguration = CreateSiteConfiguration("1000", "COVID", "COVID");
        var responseBlocks = CreateAmPmResponseBlocks(12, 0);

        _availabilityGrouper.Setup(x => x.GroupAvailability(It.IsAny<IEnumerable<TimePeriod>>(), It.IsAny<int>()))
            .Returns(responseBlocks);
        _availabilityCalculator.Setup(x => x.CalculateAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(blocks.AsEnumerable());
        _siteConfigurationService.Setup(x => x.GetSiteConfigurationAsync(It.IsAny<string>()))
            .ReturnsAsync(siteConfiguration);
        
        var request = new QueryAvailabilityRequest(
            new[] { "1000" },
            "OTHER_SERVICE", 
            "2077-01-01",
            "2077-01-01",
            QueryType.Days);

        var httpRequest = CreateRequest(request);
        
        var result = await _sut.RunAsync(httpRequest) as ContentResult;
        result.StatusCode.Should().Be(200);
        var response = ReadResponseAsync<QueryAvailabilityResponse>(result.Content);
        response.Result.Should().BeEmpty();
    }*/
    
    /*[Fact]
    public async Task RunAsync_ReturnsAvailability_ForSitesWithRequestedServiceConfiguration()
    {
        var blocks = AvailabilityHelper.CreateTestBlocks("10:00-11:00");
        var siteConfigurationA = CreateSiteConfiguration("1000", "COVID", "COVID");
        var siteConfigurationB = CreateSiteConfiguration("1001", "OTHER_SERVICE", "OTHER_SERVICE");
        var responseBlocks = CreateAmPmResponseBlocks(12, 0);

        _availabilityGrouper.Setup(x => x.GroupAvailability(It.IsAny<IEnumerable<TimePeriod>>(), It.IsAny<int>()))
            .Returns(responseBlocks);
        _availabilityCalculator.Setup(x => x.CalculateAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(blocks.AsEnumerable());
        _siteConfigurationService.Setup(x => x.GetSiteConfigurationAsync("1000")).ReturnsAsync(siteConfigurationA);
        _siteConfigurationService.Setup(x => x.GetSiteConfigurationAsync("1001")).ReturnsAsync(siteConfigurationB);
        
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
        response.Result.Count.Should().Be(1);
        response.Result[0].site.Should().Be("1000");
    }*/
    
    /*[Theory]
    [InlineData(QueryType.Days)]
    [InlineData(QueryType.Hours)]
    [InlineData(QueryType.Slots)]
    public async Task RunAsync_ReturnsCorrectAvailabilityGrouper_WhenCalledWithConfiguredQueryType(QueryType queryType)
    {
        var blocks = AvailabilityHelper.CreateTestBlocks("10:00-11:00");
        var siteConfiguration = CreateSiteConfiguration("1000", "COVID", "COVID");
        var responseBlocks = CreateAmPmResponseBlocks(12, 0);

        _availabilityGrouper.Setup(x => x.GroupAvailability(It.IsAny<IEnumerable<TimePeriod>>(), It.IsAny<int>()))
            .Returns(responseBlocks);
        _availabilityCalculator.Setup(x => x.CalculateAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(blocks.AsEnumerable());
        _siteConfigurationService.Setup(x => x.GetSiteConfigurationAsync(It.IsAny<string>()))
            .ReturnsAsync(siteConfiguration);
        
        var request = new QueryAvailabilityRequest(
            new[] { "1000" },
            "COVID", 
            "2077-01-01",
            "2077-01-01",
            queryType);

        var httpRequest = CreateRequest(request);

        await _sut.RunAsync(httpRequest);
        _availabilityGrouperFactory.Verify(x => x.Create(queryType), Times.Once());
    }*/

    /*[Fact]
    public async Task RunAsync_ReturnsNoAvailability_ForASiteThatIsNotConfigured()
    {
        var blocks = AvailabilityHelper.CreateTestBlocks("10:00-11:00");
        
        var responseBlocks = CreateAmPmResponseBlocks(12, 0);

        _availabilityGrouper.Setup(x => x.GroupAvailability(It.IsAny<IEnumerable<TimePeriod>>(), It.IsAny<int>()))
            .Returns(responseBlocks);
        _availabilityCalculator.Setup(x => x.CalculateAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(blocks.AsEnumerable());
        _siteConfigurationService.Setup(x => x.GetSiteConfigurationAsync(It.IsAny<string>()))
            .ThrowsAsync(new CosmosException("Resource not found", HttpStatusCode.NotFound, 0, "1", 1));
        
        var request = new QueryAvailabilityRequest(
            new[] { "1000" },
            "COVID", 
            "2077-01-01",
            "2077-01-01",
            QueryType.Days);

        var httpRequest = CreateRequest(request);
        
        var result = await _sut.RunAsync(httpRequest) as ContentResult;
        result.StatusCode.Should().Be(200);
        result.Content.Should().Be("[]");
    }*/
    
    /*[Fact]
    public async Task RunAsync_ReturnsAvailability_ForSitesThatAreConfigured()
    {
        var blocks = AvailabilityHelper.CreateTestBlocks("10:00-11:00");
        var siteConfiguration = CreateSiteConfiguration("1000", "COVID", "COVID");
        
        var responseBlocks = CreateAmPmResponseBlocks(12, 0);

        _availabilityGrouper.Setup(x => x.GroupAvailability(It.IsAny<IEnumerable<TimePeriod>>(), It.IsAny<int>()))
            .Returns(responseBlocks);
        _availabilityCalculator.Setup(x => x.CalculateAvailability(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(blocks.AsEnumerable());
        _siteConfigurationService.Setup(x => x.GetSiteConfigurationAsync("1000")).ReturnsAsync(siteConfiguration);
        _siteConfigurationService.Setup(x => x.GetSiteConfigurationAsync("1001"))
            .ThrowsAsync(new CosmosException("Resource not found", HttpStatusCode.NotFound, 0, "1", 1));
        
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
        response.Result.Count.Should().Be(1);
        response.Result[0].site.Should().Be("1000");
    }*/
    
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
