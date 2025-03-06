using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Functions;

public class GetSiteMetaDataFunctionTests
{
    private readonly Mock<ILogger<GetSiteMetaDataFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly Mock<ISiteService> _siteService = new();
    private readonly GetSiteMetaDataFunction _sut;
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IValidator<SiteBasedResourceRequest>> _validator = new();

    public GetSiteMetaDataFunctionTests()
    {
        _sut = new GetSiteMetaDataFunction(_siteService.Object, _validator.Object, _userContextProvider.Object,
            _logger.Object, _metricsRecorder.Object);
        _validator
            .Setup(x => x.ValidateAsync(It.IsAny<SiteBasedResourceRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task RunAsync_ReturnsNotFound_WhenRequestedSiteIsNotConfigured()
    {
        var request = CreateRequest();
        var result = await _sut.RunAsync(request, functionContext: null) as ContentResult;
        result.StatusCode.Should().Be(404);
    }

    [Theory]
    [InlineData("site_details/info_for_citizen", "Test information", "Test information")]
    [InlineData("attr_one/test_attr", "Another test", "")]
    public async Task RunAsync_ReturnsInformationForCitizen(string attrId, string attrVal, string expectedInformation)
    {
        _siteService.Setup(x => x.GetSiteByIdAsync("6877d86e-c2df-4def-8508-e1eccf0ea6ba", "site_details"))
            .ReturnsAsync(new Site
            (
                Id: "6877d86e-c2df-4def-8508-e1eccf0ea6ba",
                Name: "Test 123",
                Address: "1 Test Street",
                PhoneNumber: "0113 1111111",
                OdsCode: "15N",
                Region: "R1",
                IntegratedCareBoard: "ICB1",
                InformationForCitizens: "InformationForCitizens 123",
                Accessibilities: [new(attrId, attrVal)],
                Location: new Location("Test", [123.1, 321.3])
            ));
        var request = CreateRequest();

        var result = await _sut.RunAsync(request, functionContext: null) as ContentResult;

        result.StatusCode.Should().Be(200);

        var response = await ReadResponseAsync<GetSiteMetaDataResponse>(result.Content);

        response.AdditionalInformation.Should().Be(expectedInformation);
        response.Site.Should().Be("Test 123");
    }

    private static HttpRequest CreateRequest()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.QueryString = new QueryString("?site=6877d86e-c2df-4def-8508-e1eccf0ea6ba");
        request.Headers.Append("Authorization", "Test 123");
        return request;
    }

    private static async Task<TRequest> ReadResponseAsync<TRequest>(string response)
    {
        var body = await new StringReader(response).ReadToEndAsync();
        return JsonConvert.DeserializeObject<TRequest>(body);
    }
}
