using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using System.Net;

namespace Nhs.Appointments.Api.Tests.Functions;
public class SetSiteReferenceDetailsFunctionTests
{
    private readonly Mock<ILogger<SetSiteReferenceDetailsFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly Mock<ISiteService> _siteService = new();
    private readonly Mock<IUserContextProvider> _userContext = new();
    private readonly Mock<IValidator<SetSiteReferenceDetailsRequest>> _validator = new();

    private readonly SetSiteReferenceDetailsFunctionTestProxi _sut;

    public SetSiteReferenceDetailsFunctionTests()
    {
        _sut = new SetSiteReferenceDetailsFunctionTestProxi(
            _siteService.Object,
            _validator.Object,
            _userContext.Object,
            _logger.Object,
            _metricsRecorder.Object);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task InvokeSiteService_WhenSettingSiteReferenceDetails(bool operationSuccess)
    {
        var userPrincipal = UserDataGenerator.CreateUserPrincipal("test.user3@nhs.net");
        var site = "test-site";
        var odsCode = "ODS1";
        var icb = "ICB1";
        var region = "R1";
        var request = new SetSiteReferenceDetailsRequest(site, odsCode, icb, region);
        var operationalResult = new OperationResult(operationSuccess);

        _siteService.Setup(x => x.UpdateSiteReferenceDetailsAsync(site, odsCode, icb, region))
            .ReturnsAsync(operationalResult);
        _siteService.Setup(x => x.GetSiteByIdAsync(site, It.IsAny<string>()))
            .ReturnsAsync(new Site(
                    site,
                    "Test Site",
                    "Test Address",
                    "01234567890",
                    odsCode,
                    region,
                    icb,
                    string.Empty,
                    new List<Accessibility>
                    {
                        new("test_acces/one", "true")
                    },
                    new Location("Coords", [1.234, 5.678]),
                    null,
                    false,
                    string.Empty
                    ));

        var result = await _sut.Invoke(request);

        Assert.Multiple(
            () => result.IsSuccess.Should().Be(operationSuccess),
            () => _siteService.Verify(x => x.UpdateSiteReferenceDetailsAsync(site, odsCode, icb, region), Times.Once)
        );
    }

    [Fact]
    public async Task DoesNotInvokeService_WhenSiteIsInactive()
    {
        var site = "test-site";
        var odsCode = "ODS1";
        var icb = "ICB1";
        var region = "R1";
        var request = new SetSiteReferenceDetailsRequest(site, odsCode, icb, region);

        _siteService.Setup(x => x.GetSiteByIdAsync("test-site", It.IsAny<string>()))
            .ReturnsAsync(null as Site);

        var result = await _sut.Invoke(request);

        Assert.Multiple(
            () => result.IsSuccess.Should().BeFalse(),
            () => result.StatusCode.Should().Be(HttpStatusCode.NotFound),
            () => _siteService.Verify(x => x.UpdateSiteReferenceDetailsAsync(site, odsCode, icb, region), Times.Never)
        );
    }

    private class SetSiteReferenceDetailsFunctionTestProxi(
        ISiteService siteService,
        IValidator<SetSiteReferenceDetailsRequest> validator,
        IUserContextProvider userContextProvider,
        ILogger<SetSiteReferenceDetailsFunction> logger,
        IMetricsRecorder metricsRecorder)
        : SetSiteReferenceDetailsFunction(siteService, validator, userContextProvider, logger, metricsRecorder)
    {
        private readonly ILogger<SetSiteReferenceDetailsFunction> _logger = logger;

        public async Task<ApiResult<EmptyResponse>> Invoke(SetSiteReferenceDetailsRequest request) =>
            await HandleRequest(request, _logger);
    }
}
