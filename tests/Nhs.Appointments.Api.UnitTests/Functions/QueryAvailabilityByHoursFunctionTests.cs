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
using Nhs.Appointments.Core.Bookings;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Sites;
using Nhs.Appointments.Core.Users;
using System.Text;

namespace Nhs.Appointments.Api.Tests.Functions;
public class QueryAvailabilityByHoursFunctionTests
{
    private readonly Mock<IBookingAvailabilityStateService> _bookingAvailabilityStateService = new();
    private readonly Mock<ILogger<QueryAvailabilityByHoursFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IValidator<AvailabilityQueryByHoursRequest>> _validator = new();
    private readonly Mock<IFeatureToggleHelper> _featureToggleHelper = new();
    private readonly Mock<ISiteService> _siteService = new();
    private readonly Mock<IAvailableSlotsFilter> _availableSlotsFilter = new();

    private readonly QueryAvailabilityByHoursFunction _sut;

    public QueryAvailabilityByHoursFunctionTests()
    {
        _sut = new QueryAvailabilityByHoursFunction(
            _bookingAvailabilityStateService.Object,
            _validator.Object,
            _userContextProvider.Object,
            _logger.Object,
            _metricsRecorder.Object,
            _availableSlotsFilter.Object,
            _siteService.Object,
            _featureToggleHelper.Object);
        _validator.Setup(x => x.ValidateAsync(It.IsAny<AvailabilityQueryByHoursRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task RunAsync_ReturnsProblemResponse_WhenFeatureDisabled()
    {
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.MultiServiceJointBookings))
            .ReturnsAsync(false);

        var payload = new AvailabilityQueryByHoursRequest(
            "test-site-123",
            [
                new() { Services = ["RSV:Adult"]}
            ],
            new DateOnly(2025, 9, 1),
            new DateOnly(2025, 10, 1));

        var result = await _sut.RunAsync(CreatRequest(payload)) as ContentResult;

        result.StatusCode.Should().Be(501);
    }

    [Fact]
    public async Task RunAsync_ReturnsNotFound_WhenSiteIsInactive()
    {
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.MultiServiceJointBookings))
            .ReturnsAsync(true);
        _siteService.Setup(x => x.GetSiteByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(null as Site);

        var payload = new AvailabilityQueryByHoursRequest(
            "34e990af-5dc9-43a6-8895-b9123216d699",
            [
                new() { Services = ["RSV:Adult"]}
            ],
            new DateOnly(2025, 9, 1),
            new DateOnly(2025, 10, 1));
        var expectedResponse = new AvailabilityByHours();

        var result = await _sut.RunAsync(CreatRequest(payload)) as ContentResult;

        result.StatusCode.Should().Be(404);

        _bookingAvailabilityStateService.Verify(b => b.GetAvailableSlots(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
        _availableSlotsFilter.Verify(a => a.FilterAvailableSlots(It.IsAny<List<SessionInstance>>(), It.IsAny<List<Attendee>>()), Times.Never);
    }

    private static HttpRequest CreatRequest(AvailabilityQueryByHoursRequest payload)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        var body = JsonConvert.SerializeObject(payload);
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        request.Headers.Append("Authorization", "Test 123");
        return request;
    }
}
