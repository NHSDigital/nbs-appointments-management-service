using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;


namespace Nhs.Appointments.Api.Tests.Functions;

public class SetAvailabilityFunctionTests
{
    private readonly SetAvailabilityFunctionTestProxy _sut;
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IValidator<SetAvailabilityRequest>> _validator = new();
    private readonly Mock<IUserContextProvider> _userContext = new();
    private readonly Mock<ILogger<SetAvailabilityFunction>> _logger = new();
    private readonly Mock<IAvailabilityService> _availabilityService = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();

    public SetAvailabilityFunctionTests()
    {
        _sut = new SetAvailabilityFunctionTestProxy(
            _availabilityService.Object,
            _validator.Object,
            _userContext.Object,
            _logger.Object,
            _metricsRecorder.Object);
    }

    [Fact]
    public async Task InvokeAvailabilityService_WhenSettingAvailability()
    {
        var userPrincipal = UserDataGenerator.CreateUserPrincipal("test.user3@nhs.net");
        _userContext.Setup(x => x.UserPrincipal)
            .Returns(userPrincipal);

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
            new DateOnly(2024, 10, 10 ),
            "test-site",
            sessions,
            ApplyAvailabilityMode.Overwrite);

        var result = await _sut.Invoke(request);

        result.IsSuccess.Should().BeTrue();

        _availabilityService.Verify(x => x.ApplySingleDateSessionAsync(request.Date, request.Site, sessions, ApplyAvailabilityMode.Overwrite, "test.user3@nhs.net"), Times.Once);
    }

    private class SetAvailabilityFunctionTestProxy(IAvailabilityService availabilityService, IValidator<SetAvailabilityRequest> validator, IUserContextProvider userContextProvider, ILogger<SetAvailabilityFunction> logger, IMetricsRecorder metricsRecorder)
        : SetAvailabilityFunction(availabilityService, validator, userContextProvider, logger, metricsRecorder)
    {
        private readonly ILogger<SetAvailabilityFunction> _logger = logger;

        public async Task<ApiResult<EmptyResponse>> Invoke(SetAvailabilityRequest request) => await HandleRequest(request, _logger);
    }
}
