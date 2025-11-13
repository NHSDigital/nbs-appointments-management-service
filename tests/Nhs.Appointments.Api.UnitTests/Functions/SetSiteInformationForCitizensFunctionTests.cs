using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Functions.HttpFunctions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Sites;
using Nhs.Appointments.Core.Users;
using System.Net;
using System.Security.Policy;
using Site = Nhs.Appointments.Core.Sites.Site;

namespace Nhs.Appointments.Api.Tests.Functions;

public class SetSiteInformationForCitizensFunctionTests
{
    private readonly Mock<ILogger<SetSiteInformationForCitizensFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly Mock<ISiteService> _siteService = new();
    private readonly SetSiteInformationForCitizensFunctionTestProxi _sut;
    private readonly Mock<IUserContextProvider> _userContext = new();
    private readonly Mock<IValidator<SetSiteInformationForCitizensRequest>> _validator = new();

    public SetSiteInformationForCitizensFunctionTests()
    {
        _sut = new SetSiteInformationForCitizensFunctionTestProxi(
            _siteService.Object,
            _validator.Object,
            _userContext.Object,
            _logger.Object,
            _metricsRecorder.Object
        );
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task InvokeSiteService_WhenSettingInformationForCitizens(bool operationSuccess)
    {
        var userPrincipal = UserDataGenerator.CreateUserPrincipal("test.user3@nhs.net");
        var site = "test-site";
        var infoForCitizens = "Some information for citizens.";
        var request = new SetSiteInformationForCitizensRequest(site, infoForCitizens);
        var operationalResult = new OperationResult(operationSuccess);

        _userContext.Setup(x => x.UserPrincipal).Returns(userPrincipal);
        _siteService.Setup(x => x.UpdateInformationForCitizens(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(operationalResult);
        _siteService.Setup(x => x.GetSiteByIdAsync(site, It.IsAny<string>()))
            .ReturnsAsync(new Site(
                    site,
                    "Test Site",
                    "Test Address",
                    "01234567890",
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
            () => _siteService.Verify(x => x.UpdateInformationForCitizens(site, infoForCitizens), Times.Once)
        );
    }

    [Fact]
    public async Task DoesNotInvokeService_WhenSiteIsInactive()
    {
        var site = "test-site";
        var infoForCitizens = "Some information for citizens.";
        var request = new SetSiteInformationForCitizensRequest(site, infoForCitizens);

        _siteService.Setup(x => x.GetSiteByIdAsync("test-site", It.IsAny<string>()))
            .ReturnsAsync(null as Site);

        var result = await _sut.Invoke(request);

        Assert.Multiple(
            () => result.IsSuccess.Should().BeFalse(),
            () => result.StatusCode.Should().Be(HttpStatusCode.NotFound),
            () => _siteService.Verify(x => x.UpdateInformationForCitizens(site, infoForCitizens), Times.Never)
        );
    }

    private class SetSiteInformationForCitizensFunctionTestProxi(
        ISiteService siteService,
        IValidator<SetSiteInformationForCitizensRequest> validator,
        IUserContextProvider userContextProvider,
        ILogger<SetSiteInformationForCitizensFunction> logger,
        IMetricsRecorder metricsRecorder)
        : SetSiteInformationForCitizensFunction(siteService, validator, userContextProvider, logger, metricsRecorder)
    {
        private readonly ILogger<SetSiteInformationForCitizensFunction> _logger = logger;

        public async Task<ApiResult<EmptyResponse>> Invoke(SetSiteInformationForCitizensRequest request) =>
            await HandleRequest(request, _logger);
    }
}
