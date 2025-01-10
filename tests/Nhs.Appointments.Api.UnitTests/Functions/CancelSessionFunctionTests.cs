using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Auth;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using System.Security.Cryptography.Xml;
using System.Text;

namespace Nhs.Appointments.Api.Tests.Functions;
public class CancelSessionFunctionTests
{
    private readonly Mock<IAvailabilityService> _availabilityService = new();
    private readonly Mock<IBookingsService> _bookingService = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IValidator<CancelSessionRequest>> _validator = new();
    private readonly Mock<ILogger<CancelSessionFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();

    private readonly CancelSessionFunction _sut;

    public CancelSessionFunctionTests()
    {
        _sut = new CancelSessionFunction(
            _availabilityService.Object,
            _bookingService.Object,
            _validator.Object,
            _userContextProvider.Object,
            _logger.Object,
            _metricsRecorder.Object);
        _validator.Setup(x => x.ValidateAsync(It.IsAny<CancelSessionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task RunAsync_ReturnsSuccessResponse_WhenSessionCancelled()
    {
        var cancelSessionRequest = new CancelSessionRequest(
            "TEST01",
            new DateOnly(2025, 1, 10),
            "09:00",
            "12:00",
            ["RSV:Adult"], 5, 2);

        _availabilityService.Setup(x => x.GetSession(
            cancelSessionRequest.Site,
            It.IsAny<DateOnly>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            cancelSessionRequest.Services,
            cancelSessionRequest.SlotLength,
            cancelSessionRequest.Capacity))
            .ReturnsAsync(new SessionInstance(DateTime.Today.AddDays(1), DateTime.Today.AddDays(2))
            {
                Capacity = cancelSessionRequest.Capacity,
                SlotLength = cancelSessionRequest.SlotLength,
                Services = cancelSessionRequest.Services
            });

        var request = BuildRequest(cancelSessionRequest);

        var response = await _sut.RunAsync(request) as ContentResult;

        _availabilityService.Verify(x =>x.GetSession(
            cancelSessionRequest.Site,
            It.IsAny<DateOnly>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            cancelSessionRequest.Services,
            cancelSessionRequest.SlotLength,
            cancelSessionRequest.Capacity), Times.Once());
        _bookingService.Verify(x => x.OrphanAppointments(
            cancelSessionRequest.Site,
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>()), Times.Once());
    }

    [Fact]
    public async Task RunAsync_ReturnsNotFoundResponse_WhenSessionCantBeFound()
    {
        var cancelSessionRequest = new CancelSessionRequest(
            "TEST01",
            new DateOnly(2025, 1, 10),
            "09:00",
            "12:00",
            ["RSV:Adult"], 5, 2);

        _availabilityService.Setup(x => x.GetSession(
            cancelSessionRequest.Site,
            It.IsAny<DateOnly>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            cancelSessionRequest.Services,
            cancelSessionRequest.SlotLength,
            cancelSessionRequest.Capacity))
            .ReturnsAsync(() => null);

        var request = BuildRequest(cancelSessionRequest);

        var response = await _sut.RunAsync(request) as ContentResult;

        response.StatusCode.Should().Be(404);
    }

    private static HttpRequest BuildRequest(CancelSessionRequest requestBody)
    {
        var body = JsonConvert.SerializeObject(requestBody);
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        request.Headers.Append("Authorization", "Test 123");

        return request;
    }
}
