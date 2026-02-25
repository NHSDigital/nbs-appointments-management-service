using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Functions.HttpFunctions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Users;
using System.Text;

namespace Nhs.Appointments.Api.Tests.Functions;

public class CancelDateRangeFunctionTests
{
    private readonly Mock<IAvailabilityWriteService> _availabilityWriteService = new();
    private readonly Mock<IValidator<CancelDateRangeRequest>> _validator = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<ILogger<CancelDateRangeFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly Mock<IFeatureToggleHelper> _featureToggleHelper = new();

    private readonly CancelDateRangeFunction _sut;

    public CancelDateRangeFunctionTests()
    {
        _sut = new CancelDateRangeFunction(
            _availabilityWriteService.Object,
            _validator.Object,
            _userContextProvider.Object,
            _logger.Object,
            _metricsRecorder.Object,
            _featureToggleHelper.Object);

        _validator.Setup(x => x.ValidateAsync(It.IsAny<CancelDateRangeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task ReturnsNotImplemented_WhenCancelDateRangeToggledOff()
    {
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.CancelADateRange))
            .ReturnsAsync(false);

        var request = CreateRequest(false);

        var result = await _sut.RunAsync(request) as ContentResult;

        result.StatusCode.Should().Be(StatusCodes.Status501NotImplemented);

        _availabilityWriteService.Verify(x =>
            x.CancelDateRangeAsync(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public async Task ReturnsNotImplemented_WhenCancelDateRangeOn_CancelBookingsTrue_AndCancelDateRangeBookingsToggledOff()
    {
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.CancelADateRange))
            .ReturnsAsync(true);
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.CancelADateRangeWithBookings))
            .ReturnsAsync(false);

        var request = CreateRequest(true);

        var result = await _sut.RunAsync(request) as ContentResult;

        result.StatusCode.Should().Be(StatusCodes.Status501NotImplemented);

        _availabilityWriteService.Verify(x =>
            x.CancelDateRangeAsync(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public async Task ReturnsSuccess_WhenCancelDateRangeToggledOn_CancelBookingsFalse_AndCancelDateRangeWithBookingsToggledOff()
    {
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.CancelADateRange))
            .ReturnsAsync(true);
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.CancelADateRangeWithBookings))
            .ReturnsAsync(false);
        _availabilityWriteService.Setup(x => x.CancelDateRangeAsync(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), false, false))
            .ReturnsAsync((10, 0, 0));

        var request = CreateRequest(false);

        var result = await _sut.RunAsync(request) as ContentResult;

        result.StatusCode.Should().Be(StatusCodes.Status200OK);

        var resposneBody = await new StringReader(result.Content).ReadToEndAsync();
        var deserialisedResponse = JsonConvert.DeserializeObject<CancelDateRangeResponse>(resposneBody);

        deserialisedResponse.BookingsWithoutContactDetailsCount.Should().Be(0);
        deserialisedResponse.CancelledBookingsCount.Should().Be(0);
        deserialisedResponse.CancelledSessionsCount.Should().Be(10);

        _availabilityWriteService.Verify(x =>
            x.CancelDateRangeAsync(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), false, false),
            Times.Once);
    }

    [Fact]
    public async Task ReturnsSuccess_WhenCancelDateRangeToggledOn_CancelBookingsFalse_AndCancelDateRangeWithBookingsToggledOn()
    {
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.CancelADateRange))
            .ReturnsAsync(true);
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.CancelADateRangeWithBookings))
            .ReturnsAsync(true);
        _availabilityWriteService.Setup(x => x.CancelDateRangeAsync(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), true, true))
            .ReturnsAsync((10, 10, 5));

        var request = CreateRequest(true);

        var result = await _sut.RunAsync(request) as ContentResult;

        result.StatusCode.Should().Be(StatusCodes.Status200OK);

        var resposneBody = await new StringReader(result.Content).ReadToEndAsync();
        var deserialisedResponse = JsonConvert.DeserializeObject<CancelDateRangeResponse>(resposneBody);

        deserialisedResponse.BookingsWithoutContactDetailsCount.Should().Be(5);
        deserialisedResponse.CancelledBookingsCount.Should().Be(10);
        deserialisedResponse.CancelledSessionsCount.Should().Be(10);

        _availabilityWriteService.Verify(x =>
            x.CancelDateRangeAsync(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), true, true),
            Times.Once);
    }

    private static HttpRequest CreateRequest(bool cancelBookings)
    {
        var payload = new
        {
            site = "test-site-123",
            from = new DateOnly(2026, 2, 22),
            to = new DateOnly(2026, 3, 22),
            cancelBookings
        };

        var body = JsonConvert.SerializeObject(payload);
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        request.Headers.Append("Authorization", "Test 123");

        return request;
    }
}
