using System.Text;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.UnitTests;

namespace Nhs.Appointments.Api.Tests.Functions;

[MockedFeatureToggle(Flags.MultipleServices, false)]
public class CancelSessionFunctionTests : FeatureToggledTests
{
    private readonly Mock<IAvailabilityWriteService> _availabilityWriteService = new();
    private readonly Mock<ILogger<CancelSessionFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();

    private readonly CancelSessionFunction _sut;
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IValidator<CancelSessionRequest>> _validator = new();

    public CancelSessionFunctionTests() : base(typeof(CancelSessionFunctionTests))
    {
        _sut = new CancelSessionFunction(
            _availabilityWriteService.Object,
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

        var request = BuildRequest(cancelSessionRequest);

        var response = await _sut.RunAsync(request) as ContentResult;

        _availabilityWriteService.Verify(x => x.CancelSession(
            cancelSessionRequest.Site,
            It.IsAny<DateOnly>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            cancelSessionRequest.Services,
            cancelSessionRequest.SlotLength,
            cancelSessionRequest.Capacity), Times.Once());
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
