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
using Nhs.Appointments.Core.Features;
using System.Text;

namespace Nhs.Appointments.Api.Tests.Functions;
public class QuerySitesFunctionTests
{
    private readonly Mock<ILogger<QuerySitesFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly Mock<ISiteService> _siteService = new();
    private readonly Mock<IFeatureToggleHelper> _featureToggleHelper = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IValidator<QuerySitesRequest>> _validator = new();

    private readonly QuerySitesFunction _sut;

    public QuerySitesFunctionTests()
    {
        _sut = new QuerySitesFunction(
            _siteService.Object,
            _featureToggleHelper.Object,
            _validator.Object,
            _userContextProvider.Object,
            _logger.Object,
            _metricsRecorder.Object);
        _validator.Setup(x => x.ValidateAsync(It.IsAny<QuerySitesRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task RunAsync_ShouldReturnNotImplemented_WhenFeatureToggleDisabled()
    {
        var request = CreateRequest();

        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.QuerySites))
            .ReturnsAsync(false);

        var result = await _sut.RunAsync(request) as ContentResult;

        result.StatusCode.Should().Be(501);
    }

    [Fact]
    public async Task RunAsync_CallsSitesService_AndReturnsSuccessResponse()
    {
        var request = CreateRequest();

        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.QuerySites))
            .ReturnsAsync(true);

        var result = await _sut.RunAsync(request) as ContentResult;

        result.StatusCode.Should().Be(200);

        _siteService.Verify(x => x.QuerySitesAsync(It.IsAny<SiteFilter[]>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once);
    }

    private static HttpRequest CreateRequest()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;

        var payload = new
        {
            maxRecords = 50,
            filters = new[]
            {
                new
                {
                    latitude = -1.663,
                    longitude = 53.7966,
                    searchRadius = 4000,
                    accessNeeds = new[] {
                        "test_access_needs"
                    },
                    services = new[] {
                        "test_service"
                    },
                    from = "2025-10-20",
                    until = "2025-11-30"
                }
            }            
        };

        var body = JsonConvert.SerializeObject(payload);
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        request.Headers.Append("Authorization", "Test 123");
        return request;
    }
}
