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
using System.Web.Http;

namespace Nhs.Appointments.Api.Tests.Functions;

public class CancelBookingFunctionTests : FeatureToggledTests
{
    private readonly Mock<IBookingWriteService> _bookingWriteService = new();
    private readonly Mock<ILogger<CancelBookingFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly CancelBookingFunction _sut;
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IValidator<CancelBookingRequest>> _validator = new();

    public CancelBookingFunctionTests() : base(typeof(CancelBookingFunctionTests))
    {
        _sut = new CancelBookingFunction(_bookingWriteService.Object, _validator.Object,
            _userContextProvider.Object, _logger.Object, _metricsRecorder.Object);
        _validator.Setup(x => x.ValidateAsync(It.IsAny<CancelBookingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _bookingWriteService.Setup(x => x.CancelBooking(null, string.Empty, CancellationReason.CancelledByCitizen))  
            .Returns(Task.FromResult(BookingCancellationResult.NotFound));
    }

    [Fact]
    public async Task RunAsync_ReturnsSuccessResponse_WhenBookingCancelled()
    {
        var bookingRef = "some-booking";
        var site = "TEST01";
        _bookingWriteService.Setup(x => x.CancelBooking(bookingRef, site, CancellationReason.CancelledByCitizen)) 
            .Returns(Task.FromResult(BookingCancellationResult.Success));

        var request = BuildRequest(bookingRef, site);

        var response = await _sut.RunAsync(request) as ContentResult;

        Assert.Equal(200, response.StatusCode.Value);
    }

    [Fact]
    public async Task RunAsync_CallsBookingServiceOnce_WhenCancellationReasonIsNull()
    {
        var bookingRef = "some-booking";
        var site = "TEST01";
        _bookingWriteService.Setup(x => x.CancelBooking(bookingRef, site, CancellationReason.CancelledByCitizen)) 
            .Returns(Task.FromResult(BookingCancellationResult.Success)).Verifiable(Times.Once);

        var request = BuildRequest(bookingRef, site);

        var response = await _sut.RunAsync(request) as ContentResult;

        _bookingWriteService.Verify();
    }

    [Fact]
    public async Task RunAsync_ReturnsNotFoundResponse_WhenBookingInvalid()
    {
        var bookingRef = "some-booking";
        var site = "TEST01";
        _bookingWriteService.Setup(x => x.CancelBooking(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationReason>()))
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
        _bookingWriteService.Setup(x => x.CancelBooking(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationReason>()))
            .Returns(Task.FromResult((BookingCancellationResult)invalidResultCode));

        var request = BuildRequest(bookingRef, site);

        var response = await _sut.RunAsync(request) as InternalServerErrorResult;

        Assert.NotNull(response);
        Assert.Equal(500, response.StatusCode);
    }

    [Theory]
    [InlineData(null, CancellationReason.CancelledByCitizen)]
    [InlineData("cancelledByCitizen", CancellationReason.CancelledByCitizen)]
    [InlineData("cancelledBySite", CancellationReason.CancelledBySite)]
    public async Task RunAsync_PassesCancellationReasonToService(string queryReason, CancellationReason expectedCancellationReason)
    {
        var bookingRef = "some-booking";
        var site = "TEST01";
        var cancellationReason = "CancelledByCitizen";

        _bookingWriteService.Setup(x => x.CancelBooking(bookingRef, site, expectedCancellationReason))
            .Returns(Task.FromResult(BookingCancellationResult.Success)).Verifiable();

        var request = BuildRequest(bookingRef, site, queryReason);

        var response = await _sut.RunAsync(request) as ContentResult;

        Assert.Equal(200, response.StatusCode.Value);
        _bookingWriteService.Verify();
    }

    private static HttpRequest BuildRequest(string reference, string site, string? cancellationReason = null)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.RouteValues = new RouteValueDictionary { { "bookingReference", reference } };

        var query = $"?site={site}";
        if (!string.IsNullOrWhiteSpace(cancellationReason))
        {
            query += $"&cancellationReason={Uri.EscapeDataString(cancellationReason)}";
        }

        request.QueryString = new QueryString(query);
        request.Headers.Append("Authorization", "Test 123");

        return request;
    }
}
