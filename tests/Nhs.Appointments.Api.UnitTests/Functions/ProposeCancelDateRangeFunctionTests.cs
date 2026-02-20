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
using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Users;
using System.Text;

namespace Nhs.Appointments.Api.Tests.Functions;

public class ProposeCancelDateRangeFunctionTests
{
    private readonly Mock<IBookingAvailabilityStateService> _bookingAvailabilityStateService = new();
    private readonly Mock<ILogger<ProposeCancelDateRangeFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IFeatureToggleHelper> _featureToggleHelper = new();
    private readonly Mock<IValidator<ProposeCancelDateRangeRequest>> _validator = new();

    private readonly ProposeCancelDateRangeFunction _sut;

    public ProposeCancelDateRangeFunctionTests()
    {
        _sut = new ProposeCancelDateRangeFunction(
            _bookingAvailabilityStateService.Object,
            _validator.Object,
            _userContextProvider.Object,
            _logger.Object,
            _metricsRecorder.Object,
            _featureToggleHelper.Object
        );

        _validator.Setup(x => x.ValidateAsync(It.IsAny<ProposeCancelDateRangeRequest>(), It.IsAny<CancellationToken>()))
         .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task ReturnsNotImplementedResponse_WhenFeatureFlagIsDisabled()
    {
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.CancelADateRange))
            .ReturnsAsync(false);

        var request = CreateRequest();
        var result = await _sut.RunAsync(request) as ContentResult;

        result.StatusCode.Should().Be(StatusCodes.Status501NotImplemented);
    }

    [Fact]
    public async Task ReturnsSessionAndBookingsCountForDateRange()
    {
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.CancelADateRange))
            .ReturnsAsync(true);
        _bookingAvailabilityStateService.Setup(
            x => x.GenerateCancelDateRangeProposalActionMetricsAsync(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync((30, 150));

        var request = CreateRequest();
        var result = await _sut.RunAsync(request) as ContentResult;

        result.StatusCode.Should().Be(StatusCodes.Status200OK);
        
        var resposneBody = await new StringReader(result.Content).ReadToEndAsync();
        var deserialisedResponse = JsonConvert.DeserializeObject<ProposeCancelDateRangeResponse>(resposneBody);

        deserialisedResponse.SessionCount.Should().Be(30);
        deserialisedResponse.BookingCount.Should().Be(150);
    }

    private static HttpRequest CreateRequest()
    {
        var payload = new
        {
            site = "test-site-123",
            from = new DateOnly(2026, 2, 22),
            to = new DateOnly(2026, 3, 22)
        };

        var body = JsonConvert.SerializeObject(payload);
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        request.Headers.Append("Authorization", "Test 123");

        return request;
    }
}
