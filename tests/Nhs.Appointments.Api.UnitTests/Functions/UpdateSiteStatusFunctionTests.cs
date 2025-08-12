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
public class UpdateSiteStatusFunctionTests
{
    private readonly Mock<ISiteService> _siteService = new();
    private readonly Mock<IValidator<SetSiteStatusRequest>> _validator = new();
    private readonly Mock<ILogger<UpdateSiteStatusFunction>> _logger = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly Mock<IFeatureToggleHelper> _featureToggleHelper = new();

    private readonly UpdateSiteStatusFunction _sut;

    public UpdateSiteStatusFunctionTests()
    {
        _sut = new UpdateSiteStatusFunction(
            _siteService.Object,
            _validator.Object,
            _logger.Object,
            _userContextProvider.Object,
            _metricsRecorder.Object,
            _featureToggleHelper.Object);
    }

    [Fact]
    public async Task RunAsync_ReturnsNotImplemented_WhenFeatureToggleIsDisabled()
    {
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.SiteStatus)).ReturnsAsync(false);

        var result = await _sut.RunAsync(CreateRequest("site-id", SiteStatus.Offline)) as ContentResult;

        result.StatusCode.Should().Be(501);
    }

    [Fact]
    public async Task RunAsync_ReturnsBadRequest_WhenSiteStatusNotProvided()
    {
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.SiteStatus)).ReturnsAsync(true);

        var result = await _sut.RunAsync(CreateRequest("site-id", null)) as ContentResult;

        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task RunAsync_UpdatesSiteStatus()
    {
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.SiteStatus)).ReturnsAsync(true);
        _validator.Setup(x => x.ValidateAsync(It.IsAny<SetSiteStatusRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _siteService.Setup(x => x.SetSiteStatus(It.IsAny<string>(), It.IsAny<SiteStatus>()))
            .ReturnsAsync(new OperationResult(true));

        _ = await _sut.RunAsync(CreateRequest("site-id", SiteStatus.Offline)) as ContentResult;

        _siteService.Verify(x => x.SetSiteStatus("site-id", SiteStatus.Offline), Times.Once);
    }

    private static HttpRequest CreateRequest(string siteId, SiteStatus? siteStatus)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;

        var payload = new { 
            site = siteId,
            status = siteStatus
        };
        var body = JsonConvert.SerializeObject(payload);
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        request.Headers.Append("Authorization", "Test 123");

        return request;
    }
}
