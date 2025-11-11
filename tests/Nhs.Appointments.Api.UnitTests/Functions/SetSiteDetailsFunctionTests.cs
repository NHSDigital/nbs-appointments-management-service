using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using System.Net;

namespace Nhs.Appointments.Api.Tests.Functions;
public class SetSiteDetailsFunctionTests
{
    private readonly Mock<ILogger<SetSiteDetailsFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly Mock<ISiteService> _siteService = new();
    private readonly Mock<IUserContextProvider> _userContext = new();
    private readonly Mock<IValidator<SetSiteDetailsRequest>> _validator = new();

    private readonly SetSiteDetailsFunctionTestProxi _sut;

    public SetSiteDetailsFunctionTests()
    {
        _sut = new SetSiteDetailsFunctionTestProxi(
            _siteService.Object,
            _validator.Object,
            _userContext.Object,
            _logger.Object,
            _metricsRecorder.Object);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task InvokeSiteService_WhenSettingSiteDetails(bool operationSuccess)
    {
        var site = "test-site";
        var name = "Test Name";
        var address = "Test address";
        var phoneNo = "01234567890";
        var longitude = 1.234;
        var latitude = 4.567;
        var request = new SetSiteDetailsRequest(site, name, address, phoneNo, $"{longitude}", $"{latitude}");
        var operationalResult = new OperationResult(operationSuccess);

        _siteService.Setup(x => x.UpdateSiteDetailsAsync(site, name, address, phoneNo, decimal.Parse($"{longitude}"), decimal.Parse($"{latitude}")))
            .ReturnsAsync(operationalResult);
        _siteService.Setup(x => x.GetSiteByIdAsync(site, It.IsAny<string>()))
            .ReturnsAsync(new Site(
                    site,
                    name,
                    address,
                    phoneNo,
                    "ODS1",
                    "R1",
                    "ICB1",
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
            () => _siteService.Verify(x => x.UpdateSiteDetailsAsync(site, name, address, phoneNo, decimal.Parse($"{longitude}"), decimal.Parse($"{latitude}")),
            Times.Once)
        );
    }

    [Fact]
    public async Task DoesNotInvokeService_WhenSiteIsInactive()
    {
        var site = "test-site";
        var name = "Test Name";
        var address = "Test address";
        var phoneNo = "01234567890";
        var longitude = 1.234;
        var latitude = 4.567;
        var request = new SetSiteDetailsRequest(site, name, address, phoneNo, $"{longitude}", $"{latitude}");

        _siteService.Setup(x => x.GetSiteByIdAsync("test-site", It.IsAny<string>()))
            .ReturnsAsync(null as Site);

        var result = await _sut.Invoke(request);

        Assert.Multiple(
            () => result.IsSuccess.Should().BeFalse(),
            () => result.StatusCode.Should().Be(HttpStatusCode.NotFound),
            () => _siteService.Verify(x => x.UpdateSiteDetailsAsync(site, name, address, phoneNo, decimal.Parse($"{longitude}"), decimal.Parse($"{latitude}")),
            Times.Never)
        );
    }

    private class SetSiteDetailsFunctionTestProxi(
        ISiteService siteService,
        IValidator<SetSiteDetailsRequest> validator,
        IUserContextProvider userContextProvider,
        ILogger<SetSiteDetailsFunction> logger,
        IMetricsRecorder metricsRecorder)
        : SetSiteDetailsFunction(siteService, validator, userContextProvider, logger, metricsRecorder)
    {
        private readonly ILogger<SetSiteDetailsFunction> _logger = logger;

        public async Task<ApiResult<EmptyResponse>> Invoke(SetSiteDetailsRequest request) =>
            await HandleRequest(request, _logger);
    }
}
