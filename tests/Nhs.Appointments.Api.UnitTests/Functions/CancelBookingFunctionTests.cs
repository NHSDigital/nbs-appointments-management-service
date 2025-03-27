using System.Web.Http;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.UnitTests;

namespace Nhs.Appointments.Api.Tests.Functions;

[MockedFeatureToggle("MultiServiceAvailabilityCalculations", false)]
public class CancelBookingFunctionTests : FeatureToggledTests
{
    private readonly Mock<IBookingsService> _bookingService = new();
    private readonly Mock<IAvailabilityService> _availabilityService = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IValidator<CancelBookingRequest>> _validator = new();
    private readonly Mock<ILogger<CancelBookingFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly CancelBookingFunction _sut;

    public CancelBookingFunctionTests() : base(typeof(CancelBookingFunctionTests))
    {
        _sut = new CancelBookingFunction(_bookingService.Object, _availabilityService.Object, _validator.Object,
            _userContextProvider.Object, _logger.Object, _metricsRecorder.Object, _featureToggleHelper.Object);
        _validator.Setup(x => x.ValidateAsync(It.IsAny<CancelBookingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _bookingService.Setup(x => x.CancelBooking(null, string.Empty))
            .Returns(Task.FromResult(BookingCancellationResult.NotFound));
    }

    [Fact]
    public async Task RunAsync_ReturnsSuccessResponse_WhenBookingCancelled()
    {
        var bookingRef = "some-booking";
        var site = "TEST01";
        _bookingService.Setup(x => x.CancelBooking(bookingRef, site))
            .Returns(Task.FromResult(BookingCancellationResult.Success));

        var request = BuildRequest(bookingRef, site);

        var response = await _sut.RunAsync(request) as ContentResult;

        Assert.Equal(200, response.StatusCode.Value);
    }

    [Fact]
    public async Task RunAsync_CallsBookingServiceOnce_WhenBookingCancelled()
    {
        var bookingRef = "some-booking";
        var site = "TEST01";
        _bookingService.Setup(x => x.CancelBooking(bookingRef, site))
            .Returns(Task.FromResult(BookingCancellationResult.Success)).Verifiable(Times.Once);

        var request = BuildRequest(bookingRef, site);

        var response = await _sut.RunAsync(request) as ContentResult;

        _bookingService.Verify();
    }

    [Fact]
    public async Task RunAsync_ReturnsNotFoundResponse_WhenBookingInvalid()
    {
        var bookingRef = "some-booking";
        var site = "TEST01";
        _bookingService.Setup(x => x.CancelBooking(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult(BookingCancellationResult.NotFound));

        var request = BuildRequest(bookingRef, site);

        var response = await _sut.RunAsync(request) as ContentResult;

        Assert.Equal(404, response.StatusCode.Value);
    }

    [Fact]
    public async Task RunAsync_Fails_WhenServiceReturnsUnexpected()
    {
        var bookingRef = "some-booking";
        var site = "TEST01";
        var invalidResultCode = 99;
        _bookingService.Setup(x => x.CancelBooking(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.FromResult((BookingCancellationResult)invalidResultCode));

        var request = BuildRequest(bookingRef, site);

        var response = await _sut.RunAsync(request) as InternalServerErrorResult;

        Assert.NotNull(response);
        Assert.Equal(500, response.StatusCode);
    }

    private static HttpRequest BuildRequest(string reference, string site)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.RouteValues = new RouteValueDictionary { { "bookingReference", reference } };
        request.QueryString = new QueryString($"?site={site}");
        request.Headers.Append("Authorization", "Test 123");

        return request;
    }
}
