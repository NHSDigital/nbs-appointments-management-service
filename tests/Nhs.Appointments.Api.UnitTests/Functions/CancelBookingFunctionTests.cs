using System.Text;
using System.Web.Http;
using FluentAssertions;
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

public class CancelBookingFunctionTests : FeatureToggledTests
{
    private readonly Mock<IBookingWriteService> _bookingWriteService = new();
    private readonly Mock<ILogger<CancelBookingFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly CancelBookingFunction _sut;
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IValidator<CancelBookingRequest>> _validator = new();
    private readonly Mock<ISiteService> _siteService = new();

    public CancelBookingFunctionTests() : base(typeof(CancelBookingFunctionTests))
    {
        _sut = new CancelBookingFunction(
            _bookingWriteService.Object,
            _validator.Object,
            _userContextProvider.Object,
            _logger.Object,
            _metricsRecorder.Object,
            _siteService.Object);
        _validator.Setup(x => x.ValidateAsync(It.IsAny<CancelBookingRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _bookingWriteService.Setup(x =>
                x.CancelBooking(null, string.Empty, CancellationReason.CancelledByCitizen, It.IsAny<object>(), It.IsAny<bool>()))
            .Returns(Task.FromResult(BookingCancellationResult.NotFound));
    }

    [Fact]
    public async Task RunAsync_ReturnsSuccessResponse_WhenBookingCancelled()
    {
        var bookingRef = "some-booking";
        var site = "TEST01";
        _bookingWriteService.Setup(x =>
                x.CancelBooking(bookingRef, site, CancellationReason.CancelledByCitizen, It.IsAny<object>(), It.IsAny<bool>()))
            .Returns(Task.FromResult(BookingCancellationResult.Success));
        _siteService.Setup(x => x.GetSiteByIdAsync(site, It.IsAny<string>()))
            .ReturnsAsync(new Site(
                    site,
                    "test site",
                    "test address",
                    "03216549870",
                    "ODS1",
                    "R1",
                    "ICB1",
                    string.Empty,
                    new List<Accessibility>
                    {
                        new("acc_1", "true")
                    },
                    new Location("Coords", [1.234, 5.678]),
                    null,
                    false,
                    string.Empty
                    ));

        var request = BuildRequest(bookingRef, site);

        var response = await _sut.RunAsync(request) as ContentResult;

        Assert.Equal(200, response.StatusCode.Value);
    }

    [Fact]
    public async Task RunAsync_CallsBookingServiceOnce_WhenCancellationReasonIsNull()
    {
        var bookingRef = "some-booking";
        var site = "TEST01";
        _bookingWriteService.Setup(x =>
                x.CancelBooking(bookingRef, site, CancellationReason.CancelledByCitizen, It.IsAny<object>(), It.IsAny<bool>()))
            .Returns(Task.FromResult(BookingCancellationResult.Success)).Verifiable(Times.Once);
        _siteService.Setup(x => x.GetSiteByIdAsync(site, It.IsAny<string>()))
            .ReturnsAsync(new Site(
                    site,
                    "test site",
                    "test address",
                    "03216549870",
                    "ODS1",
                    "R1",
                    "ICB1",
                    string.Empty,
                    new List<Accessibility>
                    {
                        new("acc_1", "true")
                    },
                    new Location("Coords", [1.234, 5.678]),
                    null,
                    false,
                    string.Empty
                    ));

        var request = BuildRequest(bookingRef, site);

        var response = await _sut.RunAsync(request) as ContentResult;

        _bookingWriteService.Verify();
    }

    [Fact]
    public async Task RunAsync_ReturnsNotFoundResponse_WhenBookingInvalid()
    {
        var bookingRef = "some-booking";
        var site = "TEST01";
        _bookingWriteService.Setup(x => x.CancelBooking(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationReason>(), It.IsAny<object>(), It.IsAny<bool>()))
            .Returns(Task.FromResult(BookingCancellationResult.NotFound));
        _siteService.Setup(x => x.GetSiteByIdAsync(site, It.IsAny<string>()))
            .ReturnsAsync(new Site(
                    site,
                    "test site",
                    "test address",
                    "03216549870",
                    "ODS1",
                    "R1",
                    "ICB1",
                    string.Empty,
                    new List<Accessibility>
                    {
                        new("acc_1", "true")
                    },
                    new Location("Coords", [1.234, 5.678]),
                    null,
                    false,
                    string.Empty
                    ));

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
        _bookingWriteService.Setup(x => x.CancelBooking(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationReason>(), It.IsAny<object>(), It.IsAny<bool>()))
            .Returns(Task.FromResult((BookingCancellationResult)invalidResultCode));
        _siteService.Setup(x => x.GetSiteByIdAsync(site, It.IsAny<string>()))
            .ReturnsAsync(new Site(
                    site,
                    "test site",
                    "test address",
                    "03216549870",
                    "ODS1",
                    "R1",
                    "ICB1",
                    string.Empty,
                    new List<Accessibility>
                    {
                        new("acc_1", "true")
                    },
                    new Location("Coords", [1.234, 5.678]),
                    null,
                    false,
                    string.Empty
                    ));

        var request = BuildRequest(bookingRef, site);

        var response = await _sut.RunAsync(request) as InternalServerErrorResult;

        Assert.NotNull(response);
        Assert.Equal(500, response.StatusCode);
    }

    [Theory]
    [InlineData(null, CancellationReason.CancelledByCitizen)]
    [InlineData("cancelledByCitizen", CancellationReason.CancelledByCitizen)]
    [InlineData("cancelledBySite", CancellationReason.CancelledBySite)]
    public async Task RunAsync_PassesCancellationReasonToService(string cancellationReason, CancellationReason expectedCancellationReason)
    {
        var bookingRef = "some-booking";
        var site = "TEST01";

        _bookingWriteService
            .Setup(x => x.CancelBooking(bookingRef, site, expectedCancellationReason, It.IsAny<object>(), It.IsAny<bool>()))
            .Returns(Task.FromResult(BookingCancellationResult.Success)).Verifiable();
        _siteService.Setup(x => x.GetSiteByIdAsync(site, It.IsAny<string>()))
            .ReturnsAsync(new Site(
                    site,
                    "test site",
                    "test address",
                    "03216549870",
                    "ODS1",
                    "R1",
                    "ICB1",
                    string.Empty,
                    new List<Accessibility>
                    {
                        new("acc_1", "true")
                    },
                    new Location("Coords", [1.234, 5.678]),
                    null,
                    false,
                    string.Empty
                    ));

        var request = BuildRequest(bookingRef, site, cancellationReason);

        var response = await _sut.RunAsync(request) as ContentResult;

        Assert.Equal(200, response.StatusCode.Value);
        _bookingWriteService.Verify();
    }

    [Fact]
    public async Task RunAsync_ReturnsNotFound_WhenSiteHasBeenDeleted()
    {
        var bookingRef = "some-booking";
        var site = "TEST01";

        _siteService.Setup(x => x.GetSiteByIdAsync(site, It.IsAny<string>()))
            .ReturnsAsync(null as Site);

        var request = BuildRequest(bookingRef, site);

        var response = await _sut.RunAsync(request) as ContentResult;

        response.StatusCode.Should().Be(400);
        _bookingWriteService.Verify(x => x.CancelBooking(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationReason>(), It.IsAny<object>(), It.IsAny<bool>()), Times.Never);
    }

    private static HttpRequest BuildRequest(string reference, string site, string? cancellationReason = null)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.RouteValues = new RouteValueDictionary { { "bookingReference", reference } };

        var query = $"?site={site}";
        request.QueryString = new QueryString(query);
        request.Headers.Append("Authorization", "Test 123");

        if (!string.IsNullOrWhiteSpace(cancellationReason))
        {
            var jsonBody = $"{{ \"cancellationReason\": \"{cancellationReason}\" }}";
            var bodyBytes = Encoding.UTF8.GetBytes(jsonBody);
            request.Body = new MemoryStream(bodyBytes);
            request.ContentLength = bodyBytes.Length;
            request.ContentType = "application/json";
        }

        return request;
    }
}
