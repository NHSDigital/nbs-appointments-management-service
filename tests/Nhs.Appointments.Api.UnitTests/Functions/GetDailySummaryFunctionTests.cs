using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Nhs.Appointments.Api.Functions;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core;
using Nhs.Appointments.Core.Features;
using System.Net;
using System.Text.Json;

namespace Nhs.Appointments.Api.Tests.Functions;
public class GetDailySummaryFunctionTests
{
    private readonly Mock<IBookingAvailabilityStateService> _mockBookingAvailabilityStateService = new();
    private readonly Mock<ILogger<GetDailySummaryFunction>> _mockLogger = new();
    private readonly Mock<IMetricsRecorder> _mockMetricsRecorder = new();
    private readonly Mock<IUserContextProvider> _mockUserContextProvider = new();
    private readonly Mock<IValidator<GetDaySummaryRequest>> _mockValidator = new();
    private readonly Mock<IFeatureToggleHelper> _mockFeatureToggleHelper = new();

    private readonly GetDailySummaryFunction _sut;

    public GetDailySummaryFunctionTests()
    {
        _sut = new GetDailySummaryFunction(
            _mockBookingAvailabilityStateService.Object,
            _mockValidator.Object,
            _mockUserContextProvider.Object,
            _mockLogger.Object,
            _mockMetricsRecorder.Object,
            _mockFeatureToggleHelper.Object
        );
    }

    [Fact]
    public async Task RunAsync_WhenSiteParameterIsMissing_ReturnsBadRequest()
    {
        //Arrange
        var validationResult = new ValidationResult
        {
            Errors = new List<ValidationFailure>
            {
                new ValidationFailure("Site", "Site is required.")
            }
        };
        var request = CreateRequest(from: "2024-12-01");
        _mockFeatureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.MultipleServices)).ReturnsAsync(true);
        _mockValidator
            .Setup(x => x.ValidateAsync(
                It.Is<GetDaySummaryRequest>(r => string.IsNullOrEmpty(r.Site)),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(validationResult);

        //Act
        var result = await _sut.RunAsync(request);

        //Assert
        var contentResult = Assert.IsType<ContentResult>(result);
        contentResult.StatusCode.Should().Be(400);
        contentResult.Content.Contains("Site is required.");
    }

    [Fact]
    public async Task RunAsync_WhenFromParameterIsMissing_ReturnsBadRequest()
    {
        //Arrange
        var validationResult = new ValidationResult
        {
            Errors =
            [
                new ValidationFailure("From", "Provide a date in 'yyyy-MM-dd'")
            ]
        };
        var request = CreateRequest(site: "Site01");
        _mockFeatureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.MultipleServices)).ReturnsAsync(true);
        _mockValidator
            .Setup(x => x.ValidateAsync(
                It.Is<GetDaySummaryRequest>(r => string.IsNullOrEmpty(r.From)),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(validationResult);

        //Act
        var result = await _sut.RunAsync(request);

        //Assert
        var contentResult = Assert.IsType<ContentResult>(result);
        contentResult.StatusCode.Should().Be(400);
        contentResult.Content.Contains("Provide a date in 'yyyy-MM-dd'");
    }

    [Fact]
    public async Task RunAsync_ValidRequest_ReturnsOkWithDaySummary()
    {
        //Arrange
        var request = CreateRequest(site: "Site01", from: "2024-12-01");
        var daySummary = new Summary();
        _mockFeatureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.MultipleServices)).ReturnsAsync(true);
        _mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<GetDaySummaryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _mockBookingAvailabilityStateService.Setup(x => x.GetDaySummary(
            It.IsAny<string>(), It.IsAny<DateOnly>()    
        )).ReturnsAsync(daySummary);

        //Act
        var result = await _sut.RunAsync(request);

        //Assert
        var contentResult = Assert.IsType<ContentResult>(result);
        contentResult.StatusCode.Should().Be(200);
        var summary = JsonSerializer.Deserialize<Summary>(contentResult.Content);
        summary.Should().NotBeNull();
    }

    private static HttpRequest CreateRequest(string site = null, string from = null)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;

        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(site))
            queryParams.Add($"site={Uri.EscapeDataString(site)}");
        if (!string.IsNullOrEmpty(from))
            queryParams.Add($"from={Uri.EscapeDataString(from)}");

        if (queryParams.Count > 0)
            request.QueryString = new QueryString("?" + string.Join("&", queryParams));

        request.Headers.Append("Authorization", "Test 123");
        return request;
    }
}
