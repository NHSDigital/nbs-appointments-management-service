using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using System.Net;

namespace Nhs.Appointments.Api.Tests.Functions;
public class SetSiteAccessibilitiesFunctionTests
{
    private readonly Mock<ILogger<SetSiteAccessibilitiesFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly Mock<ISiteService> _siteService = new();
    private readonly Mock<IUserContextProvider> _userContext = new();
    private readonly Mock<IValidator<SetSiteAccessibilitiesRequest>> _validator = new();

    private readonly SetSiteAccessibilitiesFunctionTestProxi _sut;

    public SetSiteAccessibilitiesFunctionTests()
    {
        _sut = new SetSiteAccessibilitiesFunctionTestProxi(
            _siteService.Object,
            _validator.Object,
            _userContext.Object,
            _logger.Object,
            _metricsRecorder.Object);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task InvokeSiteService_WhenSettingSiteAccessibilities(bool operationSuccess)
    {
        var site = "test-site";
        var accessibilities = new List<Accessibility>
        {
            new("acc_1", "true"),
            new("acc_2", "false")
        };
        var request = new SetSiteAccessibilitiesRequest(site, accessibilities);
        var operationalResult = new OperationResult(operationSuccess);

        _siteService.Setup(x => x.UpdateAccessibilities(site, accessibilities))
            .ReturnsAsync(operationalResult);
        _siteService.Setup(x => x.GetSiteByIdAsync(site, It.IsAny<string>()))
            .ReturnsAsync(new Site(
                    site,
                    "test site",
                    "test address",
                    "03216549870",
                    "ODS1",
                    "R1",
                    "ICB1",
                    string.Empty,
                    accessibilities,
                    new Location("Coords", [1.234, 5.678]),
                    null,
                    false,
                    string.Empty
                    ));

        var result = await _sut.Invoke(request);

        Assert.Multiple(
            () => result.IsSuccess.Should().Be(operationSuccess),
            () => _siteService.Verify(x => x.UpdateAccessibilities(site, accessibilities),
            Times.Once)
        );
    }

    [Fact]
    public async Task DoesNotInvokeService_WhenSiteIsInactive()
    {
        var site = "test-site";
        var accessibilities = new List<Accessibility>
        {
            new("acc_1", "true"),
            new("acc_2", "false")
        };
        var request = new SetSiteAccessibilitiesRequest(site, accessibilities);

        _siteService.Setup(x => x.GetSiteByIdAsync("test-site", It.IsAny<string>()))
            .ReturnsAsync(null as Site);

        var result = await _sut.Invoke(request);

        Assert.Multiple(
            () => result.IsSuccess.Should().BeFalse(),
            () => result.StatusCode.Should().Be(HttpStatusCode.NotFound),
            () => _siteService.Verify(x => x.UpdateAccessibilities(site, accessibilities),
            Times.Never)
        );
    }

    private class SetSiteAccessibilitiesFunctionTestProxi(
        ISiteService siteService,
        IValidator<SetSiteAccessibilitiesRequest> validator,
        IUserContextProvider userContextProvider,
        ILogger<SetSiteAccessibilitiesFunction> logger,
        IMetricsRecorder metricsRecorder)
        : SetSiteAccessibilitiesFunction(siteService, validator, userContextProvider, logger, metricsRecorder)
    {
        private readonly ILogger<SetSiteAccessibilitiesFunction> _logger = logger;

    public async Task<ApiResult<EmptyResponse>> Invoke(SetSiteAccessibilitiesRequest request) =>
        await HandleRequest(request, _logger);
    }
}
