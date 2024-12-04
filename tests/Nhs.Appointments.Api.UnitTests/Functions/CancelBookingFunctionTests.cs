using System.Text;
using System.Web.Http;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;

namespace Nhs.Appointments.Api.Tests.Functions;

public class CancelBookingFunctionTests
{
    private readonly CancelBookingFunction _sut;
    private readonly Mock<IBookingsService> _bookingService = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IValidator<CancelBookingRequest>> _validator = new();
    private readonly Mock<ILogger<CancelBookingFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();

    public CancelBookingFunctionTests()
    {
        _sut = new CancelBookingFunction(_bookingService.Object, _validator.Object, _userContextProvider.Object, _logger.Object, _metricsRecorder.Object);
        _validator.Setup(x => x.ValidateAsync(It.IsAny<CancelBookingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _bookingService.Setup(x => x.CancelBooking(null)).Returns(Task.FromResult(BookingCancellationResult.NotFound));
    }

    [Fact]
    public async Task RunAsync_ReturnsSuccessResponse_WhenBookingCancelled()
    {
        var bookingRef = "some-booking";
        _bookingService.Setup(x => x.CancelBooking(bookingRef)).Returns(Task.FromResult(BookingCancellationResult.Success));

        var request = BuildRequest(bookingRef);

        var response = await _sut.RunAsync(request) as ContentResult;

        Assert.Equal(200, response.StatusCode.Value);
    }

    [Fact]
    public async Task RunAsync_CallsBookingServiceOnce_WhenBookingCancelled()
    {
        var bookingRef = "some-booking";
        _bookingService.Setup(x => x.CancelBooking(bookingRef)).Returns(Task.FromResult(BookingCancellationResult.Success)).Verifiable(Times.Once);

        var request = BuildRequest(bookingRef);

        var response = await _sut.RunAsync(request) as ContentResult;

        _bookingService.Verify();
    }

    [Fact]
    public async Task RunAsync_ReturnsNotFoundResponse_WhenBookingInvalid()
    {
        var bookingRef = "some-booking";
        _bookingService.Setup(x => x.CancelBooking(It.IsAny<string>())).Returns(Task.FromResult(BookingCancellationResult.NotFound));

        var request = BuildRequest(bookingRef);

        var response = await _sut.RunAsync(request) as ContentResult;

        Assert.Equal(404, response.StatusCode.Value);
    }

    [Fact]
    public async Task RunAsync_Fails_WhenServiceReturnsUnexpected()
    {
        var bookingRef = "some-booking";
        var invalidResultCode = 99;
        _bookingService.Setup(x => x.CancelBooking(It.IsAny<string>())).Returns(Task.FromResult((BookingCancellationResult)invalidResultCode));

        var request = BuildRequest(bookingRef);

        var response = await _sut.RunAsync(request) as InternalServerErrorResult;

        Assert.NotNull(response);
        Assert.Equal(500, response.StatusCode);
    }

    private static HttpRequest BuildRequest(string reference)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.RouteValues = new Microsoft.AspNetCore.Routing.RouteValueDictionary { { "bookingReference", reference } };
        request.Headers.Append("Authorization", "Test 123");

        return request;
    }
}