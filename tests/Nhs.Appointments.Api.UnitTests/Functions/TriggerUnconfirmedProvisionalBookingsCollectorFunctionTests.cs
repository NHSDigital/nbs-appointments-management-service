using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Functions.HttpFunctions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Core.Users;

namespace Nhs.Appointments.Api.Tests.Functions;

public class TriggerUnconfirmedProvisionalBookingsCollectorFunctionTests
{
    private readonly Mock<IBookingWriteService> _bookingWriteService = new();
    private readonly Mock<IValidator<RemoveExpiredProvisionalBookingsRequest>> _validator = new();
    private readonly Mock<IUserContextProvider> _userContext = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly ILogger<TriggerBookingRemindersFunction> _logger =
        new LoggerFactory().CreateLogger<TriggerBookingRemindersFunction>();

    private readonly TriggerFunctionTestProxy _sut;

    public TriggerUnconfirmedProvisionalBookingsCollectorFunctionTests()
    {
        _sut = new TriggerFunctionTestProxy(
            _bookingWriteService.Object,
            _validator.Object,
            _userContext.Object,
            _logger,
            _metricsRecorder.Object);
    }

    [Fact]
    public async Task UsesDefaults_WhenRequestEmpty()
    {
        var request = new RemoveExpiredProvisionalBookingsRequest(null, null);

        _bookingWriteService
            .Setup(s => s.RemoveUnconfirmedProvisionalBookings(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new[] { "id1" });

        var result = await _sut.Invoke(request);

        result.IsSuccess.Should().BeTrue();
        result.ResponseObject.RemovedBookingRefs.Should().Contain("id1");
        _bookingWriteService.Verify(s => s.RemoveUnconfirmedProvisionalBookings(
            It.Is<int>(b => b == 200), // default
            It.Is<int>(d => d == 8)), Times.Once);
    }

    [Fact]
    public async Task UsesOverrides_WhenRequestHasValues()
    {
        var request = new RemoveExpiredProvisionalBookingsRequest(250, 10);

        _bookingWriteService
            .Setup(s => s.RemoveUnconfirmedProvisionalBookings(250, 10))
            .ReturnsAsync(new[] { "id2" });

        var result = await _sut.Invoke(request);
        result.ResponseObject.RemovedBookingRefs.Should().Contain("id2");
        _bookingWriteService.Verify(s => s.RemoveUnconfirmedProvisionalBookings(
            It.Is<int>(b => b == 250),
            It.Is<int>(d => d == 10)), Times.Once);
    }

    private class TriggerFunctionTestProxy(
        IBookingWriteService bookingWriteService,
        IValidator<RemoveExpiredProvisionalBookingsRequest> validator,
        IUserContextProvider userContextProvider,
        ILogger<TriggerBookingRemindersFunction> logger,
        IMetricsRecorder metricsRecorder)
        : TriggerUnconfirmedProvisionalBookingsCollectorFunction(
            bookingWriteService, validator, userContextProvider, logger, metricsRecorder)
    {
        private readonly ILogger<TriggerBookingRemindersFunction> _logger = logger;

        public async Task<ApiResult<RemoveExpiredProvisionalBookingsResponse>> Invoke(
            RemoveExpiredProvisionalBookingsRequest request) =>
            await HandleRequest(request, _logger);
    }
}
