using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Functions.HttpFunctions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Sites;
using Nhs.Appointments.Core.Users;
using System.Net;

namespace Nhs.Appointments.Api.Tests.Functions;

public class SetAvailabilityFunctionTests
{
    private readonly Mock<IAvailabilityWriteService> _availabilityService = new();
    private readonly Mock<ILogger<SetAvailabilityFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly SetAvailabilityFunctionTestProxy _sut;
    private readonly Mock<IUserContextProvider> _userContext = new();
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IValidator<SetAvailabilityRequest>> _validator = new();
    private readonly Mock<ISiteService> _siteService = new();

    public SetAvailabilityFunctionTests()
    {
        _sut = new SetAvailabilityFunctionTestProxy(
            _availabilityService.Object,
            _validator.Object,
            _userContext.Object,
            _logger.Object,
            _metricsRecorder.Object,
            _siteService.Object);
    }

    [Fact]
    public async Task InvokeAvailabilityService_WhenSettingAvailability()
    {
        var userPrincipal = UserDataGenerator.CreateUserPrincipal("test.user3@nhs.net");
        _userContext.Setup(x => x.UserPrincipal)
            .Returns(userPrincipal);
        _siteService.Setup(x => x.GetSiteByIdAsync("test-site", It.IsAny<string>()))
            .ReturnsAsync(new Site(
                    "test-site",
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

        var sessions = new List<Session>
        {
            new()
            {
                Capacity = 1,
                From = TimeOnly.FromDateTime(new DateTime(2024, 10, 10, 9, 0, 0)),
                Until = TimeOnly.FromDateTime(new DateTime(2024, 10, 10, 16, 0, 0)),
                Services = ["RSV", "COVID"],
                SlotLength = 5
            }
        }.ToArray();

        var request = new SetAvailabilityRequest(
            new DateOnly(2024, 10, 10),
            "test-site",
            sessions,
            ApplyAvailabilityMode.Overwrite);

        var result = await _sut.Invoke(request);

        result.IsSuccess.Should().BeTrue();

        _availabilityService.Verify(
            x => x.ApplySingleDateSessionAsync(request.Date, request.Site, sessions, ApplyAvailabilityMode.Overwrite,
                "test.user3@nhs.net", null), Times.Once);
    }

    [Fact]
    public async Task DoesNotInvokeAvailabilityService_WhenSiteIsInactive()
    {
        _siteService.Setup(x => x.GetSiteByIdAsync("test-site", It.IsAny<string>()))
            .ReturnsAsync(null as Site);
        
        var sessions = new List<Session>
        {
            new()
            {
                Capacity = 1,
                From = TimeOnly.FromDateTime(new DateTime(2024, 10, 10, 9, 0, 0)),
                Until = TimeOnly.FromDateTime(new DateTime(2024, 10, 10, 16, 0, 0)),
                Services = ["RSV", "COVID"],
                SlotLength = 5
            }
        }.ToArray();

        var request = new SetAvailabilityRequest(
            new DateOnly(2024, 10, 10),
            "test-site",
            sessions,
            ApplyAvailabilityMode.Overwrite);

        var result = await _sut.Invoke(request);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);

        _availabilityService.Verify(
            x => x.ApplySingleDateSessionAsync(request.Date, request.Site, sessions, ApplyAvailabilityMode.Overwrite,
                "test.user3@nhs.net", null), Times.Never);
    }

    private class SetAvailabilityFunctionTestProxy(
        IAvailabilityWriteService availabilityWriteService,
        IValidator<SetAvailabilityRequest> validator,
        IUserContextProvider userContextProvider,
        ILogger<SetAvailabilityFunction> logger,
        IMetricsRecorder metricsRecorder,
        ISiteService siteService)
        : SetAvailabilityFunction(availabilityWriteService, validator, userContextProvider, logger, metricsRecorder, siteService)
    {
        private readonly ILogger<SetAvailabilityFunction> _logger = logger;

        public async Task<ApiResult<EmptyResponse>> Invoke(SetAvailabilityRequest request) =>
            await HandleRequest(request, _logger);
    }
}
