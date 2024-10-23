using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Availability;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using System.Net;

namespace Nhs.Appointments.Api.Tests.Functions;

public class SetAvailabilityFunctionTests
{
    private readonly SetAvailabilityFunctionTestProxy _sut;
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IValidator<SetAvailabilityRequest>> _validator = new();
    private readonly Mock<IUserContextProvider> _userContext = new();
    private readonly Mock<ILogger<SetAvailabilityFunction>> _logger = new();
    private readonly Mock<IAvailabilityService> _availabilityService = new();

    public SetAvailabilityFunctionTests()
    {
        _sut = new SetAvailabilityFunctionTestProxy(
            _availabilityService.Object,
            _validator.Object,
            _userContext.Object,
            _logger.Object);
    }

    [Fact]
    public async Task InvokeAvailabilityService_WhenSettingAvailability()
    {
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
            "2024-10-10",
            "test-site",
            sessions);

        var result = await _sut.Invoke(request);

        result.IsSuccess.Should().BeTrue();

        _availabilityService.Verify(x => x.SetAvailabilityAsync(request.AvailabilityDate, request.Site, sessions), Times.Once);
    }

    internal class SetAvailabilityFunctionTestProxy : SetAvailabilityFunction
    {
        private ILogger<SetAvailabilityFunction> _logger;

        public SetAvailabilityFunctionTestProxy(
            IAvailabilityService availabilityService,
            IValidator<SetAvailabilityRequest> validator,
            IUserContextProvider userContextProvider,
            ILogger<SetAvailabilityFunction> logger)
            : base(availabilityService, validator, userContextProvider, logger)
        {
            _logger = logger;
        }

        public async Task<ApiResult<EmptyResponse>> Invoke(SetAvailabilityRequest request) => await HandleRequest(request, _logger);
    }
}
