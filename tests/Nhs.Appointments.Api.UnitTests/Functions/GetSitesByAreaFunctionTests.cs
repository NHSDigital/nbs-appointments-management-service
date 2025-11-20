using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Functions.HttpFunctions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Geography;
using Nhs.Appointments.Core.Sites;
using Nhs.Appointments.Core.Users;

namespace Nhs.Appointments.Api.Tests.Functions;

public class GetSitesByAreaFunctionTests
{
    private readonly Mock<ILogger<GetSitesByAreaFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly Mock<ISiteService> _siteService = new();
    private readonly GetSitesByAreaFunction _sut;
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IValidator<GetSitesByAreaRequest>> _validator = new();

    public GetSitesByAreaFunctionTests()
    {
        _sut = new GetSitesByAreaFunction(_siteService.Object, _validator.Object, _userContextProvider.Object,
            _logger.Object, _metricsRecorder.Object);
        _validator
            .Setup(x => x.ValidateAsync(It.IsAny<GetSitesByAreaRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("attr,attr2,,attr3")]
    [InlineData("attr,attr2,attr3,")]
    [InlineData("")]
    public async Task RunAsync_ReturnsBadRequest_WhenAccessNeedsContainsEmptyString(string accessNeeds)
    {
        var request = CreateRequest(34.6, 2.1, 10, 10, true, accessNeeds);
        var result = await _sut.RunAsync(request) as ContentResult;
        result?.StatusCode.Should().Be(400);
        _siteService.Verify(
            x => x.FindSitesByArea(new Coordinates { Longitude = 34.6, Latitude = 2.1 }, 10, 10, Array.Empty<string>(),
                false, null), Times.Never());
    }


    [Theory]
    [InlineData(null, null)]
    [InlineData(34.6, null)]
    [InlineData(null, 2.1)]
    public async Task RunAsync_ParsesMissingCoordinatesAsNull(double? longitude, double? latitude)
    {
        var request = CreateRequest(longitude, latitude, 10, 10, true, "attr_1");
        var result = await _sut.RunAsync(request) as ContentResult;

        _validator.Verify(
            x => x.ValidateAsync(It.Is<GetSitesByAreaRequest>(x => x.Coordinates == null),
                It.IsAny<CancellationToken>()),
            Times.Once());
    }

    [Fact]
    public async Task RunAsync_ReturnsSuccess_WhenSiteRequestedWithAccessNeeds()
    {
        const double longitude = 34.6;
        const double latitude = 2.1;
        const int searchRadius = 100000;
        const int maxRecords = 50;
        const string accessNeeds = "attr_1";
        var sites = new List<SiteWithDistance>
        {
            new(
                new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
                    Name: "Alpha",
                    Address: "somewhere",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "15N",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    InformationForCitizens: "Information For Citizens 123",
                    Accessibilities: new[] { new Accessibility(Id: "accessibility/attr_1", Value: "true") },
                    new Location("point", [0.1, 10]),
                    status : SiteStatus.Online,
                    isDeleted: null,
                    Type: null),
                Distance: 100)
        };
        _siteService
            .Setup(x => x.FindSitesByArea(new Coordinates { Longitude = longitude, Latitude = latitude }, searchRadius,
                maxRecords, new[] { accessNeeds }, false, null))
            .ReturnsAsync(sites);
        var request = CreateRequest(longitude, latitude, searchRadius, maxRecords, true, accessNeeds);
        var result = await _sut.RunAsync(request) as ContentResult;
        result?.StatusCode.Should().Be(200);
        _siteService.Verify(
            x => x.FindSitesByArea(It.Is<Coordinates>(x => x.Latitude == latitude && x.Longitude == longitude),
                searchRadius, maxRecords, new[] { accessNeeds }, false, null),
            Times.Once());
    }

    [Fact]
    public async Task RunAsync_ReturnsSuccess_WhenSitesRequestedWithoutAccessNeeds()
    {
        const double longitude = 34.6;
        const double latitude = 2.1;
        const int searchRadius = 50000;
        const int maxRecords = 50;
        var sites = new List<SiteWithDistance>
        {
            new(
                new Site(
                    Id: "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
                    Name: "Alpha",
                    Address: "somewhere",
                    PhoneNumber: "0113 1111111",
                    OdsCode: "15N",
                    Region: "R1",
                    IntegratedCareBoard: "ICB1",
                    InformationForCitizens: "Information For Citizens 123",
                    Accessibilities: new[] { new Accessibility(Id: "accessibility/attr_1", Value: "true") },
                    new Location("point", [0.1, 10]),
                    status : SiteStatus.Online,
                    isDeleted: null,
                    Type: null),
                Distance: 100)
        };
        _siteService
            .Setup(x => x.FindSitesByArea(new Coordinates { Longitude = longitude, Latitude = latitude }, searchRadius,
                maxRecords, Array.Empty<string>(), false, null))
            .ReturnsAsync(sites);
        var request = CreateRequest(longitude, latitude, searchRadius, maxRecords, false);
        var result = await _sut.RunAsync(request) as ContentResult;
        result?.StatusCode.Should().Be(200);
        _siteService.Verify(
            x => x.FindSitesByArea(It.Is<Coordinates>(x => x.Latitude == latitude && x.Longitude == longitude),
                searchRadius,
                maxRecords, Array.Empty<string>(), false, null),
            Times.Once());
    }


    private static HttpRequest CreateRequest(double? longitude, double? latitude, int searchRadius, int maxRecords,
        bool includeAccessNeedsWhenEmpty, params string[] accessNeeds)
    {
        var context = new DefaultHttpContext();
        var queryString =
            $"?searchRadius={searchRadius}&maxRecords={maxRecords}&ignoreCache=false";

        if (longitude.HasValue)
        {
            queryString += $"&long={longitude}";
        }

        if (latitude.HasValue)
        {
            queryString += $"&lat={latitude}";
        }

        if (accessNeeds.Any() || includeAccessNeedsWhenEmpty)
        {
            queryString += $"&accessNeeds={string.Join(",", accessNeeds)}";
        }

        var request = context.Request;
        request.QueryString = new QueryString(queryString);
        return request;
    }

    private static async Task<TRequest> ReadResponseAsync<TRequest>(string response)
    {
        var body = await new StringReader(response).ReadToEndAsync();
        return JsonConvert.DeserializeObject<TRequest>(body);
    }
}
