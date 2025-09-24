using FluentAssertions;
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
using System.Text;

namespace Nhs.Appointments.Api.Tests.Functions;

public class EditSessionFunctionTests
{
    private readonly Mock<IValidator<EditSessionRequest>> _mockValidator = new();
    private readonly Mock<IUserContextProvider> _mockUserContext = new();
    private readonly Mock<ILogger<EditSessionFunction>> _mockLogger = new();
    private readonly Mock<IMetricsRecorder> _mockMetricsRecorder = new();
    private readonly Mock<IFeatureToggleHelper> _mockFeatureToggleHelper = new();
    private readonly Mock<IAvailabilityWriteService> _mockAvailabilityWriteService = new();

    private readonly EditSessionFunction _sut;

    public EditSessionFunctionTests()
    {
        _sut = new EditSessionFunction(
            _mockValidator.Object,
            _mockUserContext.Object,
            _mockLogger.Object,
            _mockMetricsRecorder.Object,
            _mockAvailabilityWriteService.Object,
            _mockFeatureToggleHelper.Object);

        _mockValidator.Setup(x => x.ValidateAsync(It.IsAny<EditSessionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task RunAsync_ReturnsNoImplemented_WhenFeatureToggleIsDisabled()
    {
        var editSessionRequest = new EditSessionRequest(
            "TEST123",
            new DateOnly(2025, 10, 10),
            new DateOnly(2025, 10, 12),
            new SessionOrWildcard { IsWildcard = true, Session = null },
            null);
        var request = BuildRequest(editSessionRequest);

        _mockFeatureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.ChangeSessionUpliftedJourney))
            .ReturnsAsync(false);

        var result = await _sut.RunAsync(request) as ContentResult;

        result.StatusCode.Should().Be(501);
    }

    [Fact]
    public async Task RunAsync_ReturnsSuccessful_WhenSessionUpadted()
    {
        var editSessionRequest = new EditSessionRequest(
            "TEST123",
            new DateOnly(2025, 10, 10),
            new DateOnly(2025, 10, 12),
            new SessionOrWildcard { IsWildcard = true, Session = null },
            null);
        var request = BuildRequest(editSessionRequest);

        _mockFeatureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.ChangeSessionUpliftedJourney))
            .ReturnsAsync(true);
        _mockAvailabilityWriteService.Setup(x => x.EditOrCancelSessionAsync(
            It.IsAny<string>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>(),
            It.IsAny<Session>(),
            It.IsAny<Session>(),
            It.IsAny<bool>())).ReturnsAsync((true, string.Empty));

        var result = await _sut.RunAsync(request) as ContentResult;

        _mockAvailabilityWriteService.Verify(x => x.EditOrCancelSessionAsync(
            editSessionRequest.Site,
            editSessionRequest.From,
            editSessionRequest.To,
            It.IsAny<Session>(),
            null,
            false), Times.Once);
    }

    [Fact]
    public async Task RunAsync_ReturnsUnprocessableContent_WhenUpdateWasUnsuccessful()
    {
        var editSessionRequest = new EditSessionRequest(
            "TEST123",
            new DateOnly(2025, 10, 10),
            new DateOnly(2025, 10, 12),
            new SessionOrWildcard { IsWildcard = true, Session = null },
            null);
        var request = BuildRequest(editSessionRequest);

        _mockFeatureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.ChangeSessionUpliftedJourney))
            .ReturnsAsync(true);
        _mockAvailabilityWriteService.Setup(x => x.EditOrCancelSessionAsync(
            It.IsAny<string>(),
            It.IsAny<DateOnly>(),
            It.IsAny<DateOnly>(),
            It.IsAny<Session>(),
            It.IsAny<Session>(),
            It.IsAny<bool>())).ReturnsAsync((false, "Something went wrong"));

        var result = await _sut.RunAsync(request) as ContentResult;
        result.StatusCode.Should().Be(422);

        _mockAvailabilityWriteService.Verify(x => x.EditOrCancelSessionAsync(
            editSessionRequest.Site,
            editSessionRequest.From,
            editSessionRequest.To,
            It.IsAny<Session>(),
            null,
            false), Times.Once);
    }

    private static HttpRequest BuildRequest(EditSessionRequest requestBody)
    {
        var body = JsonConvert.SerializeObject(requestBody);
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        request.Headers.Append("Authorization", "Test 123");

        return request;
    }
}
