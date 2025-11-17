using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Nhs.Appointments.Api.Models;
using Nhs.Appointments.Core.Features;
using System.Text;
using Nhs.Appointments.Api.Functions.HttpFunctions;
using Nhs.Appointments.Core.Availability;
using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Core.Users;

namespace Nhs.Appointments.Api.Tests.Functions;
public class ProposeAvailabilityChangeFunctionTests
{
    private readonly Mock<ILogger<ProposeAvailabilityChangeFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly Mock<IBookingAvailabilityStateService> _bookingAvailabilityStateService = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IValidator<AvailabilityChangeProposalRequest>> _validator = new();
    private readonly Mock<IFeatureToggleHelper> _featureToggleHelper = new();

    private readonly ProposeAvailabilityChangeFunction _sut;

    public ProposeAvailabilityChangeFunctionTests()
    {
        _sut = new ProposeAvailabilityChangeFunction(
            _bookingAvailabilityStateService.Object, 
            _validator.Object, 
            _userContextProvider.Object,
            _logger.Object, 
            _metricsRecorder.Object,
            _featureToggleHelper.Object
        );
        _validator.Setup(x => x.ValidateAsync(It.IsAny<AvailabilityChangeProposalRequest>(), It.IsAny<CancellationToken>()))
         .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task RunAsync_ReturnsRecalculationsResult_WhenRequestIsValid()
    {
        var request = BuildRequest();
        var response = new AvailabilityUpdateProposal()
        {
            NewlySupportedBookingsCount = 1,
            NewlyOrphanedBookingsCount = 1
        };

        _bookingAvailabilityStateService.Setup(x =>
        x.GenerateSessionProposalActionMetrics(
            It.IsAny<string>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<Session>(),
            It.IsAny<Session>())
        ).ReturnsAsync(response);
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.ChangeSessionUpliftedJourney))
            .ReturnsAsync(true);

        var result = await _sut.RunAsync(request) as ContentResult;

        result.StatusCode.Should().Be(200);

        var body = await new StringReader(result.Content).ReadToEndAsync();
        var deserialisedResponse = JsonConvert.DeserializeObject<AvailabilityUpdateProposal>(body);

        deserialisedResponse.NewlySupportedBookingsCount.Should().Be(response.NewlySupportedBookingsCount);
        deserialisedResponse.NewlyOrphanedBookingsCount.Should().Be(response.NewlyOrphanedBookingsCount);
    }

    [Fact]
    public async Task RunAsync_MatchingSessionNotFound_ResultIsBadRequest()
    {
        var request = BuildRequest();
        var response = new AvailabilityUpdateProposal()
        {
            NewlySupportedBookingsCount = 1,
            NewlyOrphanedBookingsCount = 1,
            MatchingSessionNotFound = true
        };

        _bookingAvailabilityStateService.Setup(x =>
        x.GenerateSessionProposalActionMetrics(
            It.IsAny<string>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<Session>(),
            It.IsAny<Session>())
        ).ReturnsAsync(response);
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.ChangeSessionUpliftedJourney))
            .ReturnsAsync(true);

        var result = await _sut.RunAsync(request) as ContentResult;

        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task RunAsync_FeatureToggleDisabled_ResultIsNotFound()
    {
        var request = BuildRequest();
        var response = new AvailabilityUpdateProposal()
        {
            NewlySupportedBookingsCount = 1,
            NewlyOrphanedBookingsCount = 1,
            MatchingSessionNotFound = true
        };

        _bookingAvailabilityStateService.Setup(x =>
        x.GenerateSessionProposalActionMetrics(
            It.IsAny<string>(),
            It.IsAny<DateTime>(),
            It.IsAny<DateTime>(),
            It.IsAny<Session>(),
            It.IsAny<Session>())
        ).ReturnsAsync(response);
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.ChangeSessionUpliftedJourney))
            .ReturnsAsync(false);

        var result = await _sut.RunAsync(request) as ContentResult;

        result.StatusCode.Should().Be(404);
    }

    private static HttpRequest BuildRequest()
    {
        var requestBody = new
        {
            site = "Site15",
            from = "2025-10-24",
            to = "2025-10-24",
            sessionMatcher = new
            {
                from = "12:00",
                until = "16:00",
                services = new[] { "RSV:Adult" },
                slotLength = 10,
                capacity = 1,
            },
            sessionReplacement = new
            {
                from = "14:00",
                until = "16:00",
                services = new[] { "RSV:Adult" },
                slotLength = 10,
                capacity = 1,
            }
        };

        var body = JsonConvert.SerializeObject(requestBody);
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        request.Headers.Append("Authorization", "Test 123");

        return request;
    }
}
