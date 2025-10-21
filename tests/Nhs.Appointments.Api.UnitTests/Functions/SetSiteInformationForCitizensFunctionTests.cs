using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Api.Tests.Functions;

public class SetSiteInformationForCitizensFunctionTests
{
    private readonly Mock<ILogger<SetSiteInformationForCitizensFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly Mock<ISiteService> _siteService = new();
    private readonly Mock<IFeatureToggleHelper> _featureToggleHelper = new();
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
            _metricsRecorder.Object,
            _featureToggleHelper.Object
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
        _siteService.Setup(x => x.UpdateInformationForCitizens(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(operationalResult);
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.SoftDeletionOfSites)).ReturnsAsync(false);

        var result = await _sut.Invoke(request);

        Assert.Multiple(
            () => result.IsSuccess.Should().Be(operationSuccess),
            () => _siteService.Verify(x => x.UpdateInformationForCitizens(site, infoForCitizens, It.IsAny<bool>()), Times.Once)
        );
    }


    private class SetSiteInformationForCitizensFunctionTestProxi(
        ISiteService siteService,
        IValidator<SetSiteInformationForCitizensRequest> validator,
        IUserContextProvider userContextProvider,
        ILogger<SetSiteInformationForCitizensFunction> logger,
        IMetricsRecorder metricsRecorder,
        IFeatureToggleHelper featureToggleHelper)
        : SetSiteInformationForCitizensFunction(siteService, validator, userContextProvider, logger, metricsRecorder, featureToggleHelper)
    {
        private readonly ILogger<SetSiteInformationForCitizensFunction> _logger = logger;

        public async Task<ApiResult<EmptyResponse>> Invoke(SetSiteInformationForCitizensRequest request) =>
            await HandleRequest(request, _logger);
    }
}
