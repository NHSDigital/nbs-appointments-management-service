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
            new DateOnly(2025, 9, 1));

        var result = await _sut.RunAsync(CreateRequest(payload)) as ContentResult;

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
            new DateOnly(2025, 9, 1));
        var expectedResponse = new AvailabilityByHours();

        var result = await _sut.RunAsync(CreateRequest(payload)) as ContentResult;

        result.StatusCode.Should().Be(404);

        _bookingAvailabilityStateService.Verify(b => b.GetAvailableSlots(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
        _availableSlotsFilter.Verify(a => a.FilterAvailableSlots(It.IsAny<List<SessionInstance>>(), It.IsAny<List<Attendee>>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_ReturnAvailabilityByHours()
    {
        var slots = new[]
        {
            new SessionInstance(new DateTime(2077, 1, 1, 9, 0, 0), new DateTime(2077, 1, 1, 9, 5, 0)),
            new SessionInstance(new DateTime(2077, 1, 1, 10, 0, 0), new DateTime(2077, 1, 1, 10, 5, 0)),
            new SessionInstance(new DateTime(2077, 1, 1, 11, 0, 0), new DateTime(2077, 1, 1, 11, 5, 0)),
            new SessionInstance(new DateTime(2077, 1, 1, 12, 0, 0), new DateTime(2077, 1, 1, 13, 0, 0)),
        };

        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.MultiServiceJointBookings))
            .ReturnsAsync(true);
        _siteService.Setup(x => x.GetSiteByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Site(
                "2de5bb57-060f-4cb5-b14d-16587d0c2e8f",
                "Test Site",
                "Test Address",
                "01234567890",
                "ODS1",
                "R1",
                "ICB1",
                string.Empty,
                new List<Accessibility>
                {
                    new("test_acces/one", "true")
                },
                new Location("Coords", [1.234, 5.678]),
                null,
                null,
                string.Empty));
        _bookingAvailabilityStateService.Setup(x => x.GetAvailableSlots(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(slots);
        _availableSlotsFilter.Setup(x => x.FilterAvailableSlots(It.IsAny<List<SessionInstance>>(), It.IsAny<List<Attendee>>()))
            .Returns(slots);

        var payload = new AvailabilityQueryByHoursRequest(
            "34e990af-5dc9-43a6-8895-b9123216d699",
            [
                new() { Services = ["RSV:Adult"]}
            ],
            new DateOnly(2077, 1, 1));

        var result = await _sut.RunAsync(CreateRequest(payload)) as ContentResult;

        result.StatusCode.Should().Be(200);

        var body = await new StringReader(result.Content).ReadToEndAsync();
        var response = JsonConvert.DeserializeObject<AvailabilityByHours>(body);

        response.Hours.Count.Should().Be(4);
        response.Hours.First().From.Should().Be("09:00");
        response.Hours.First().Until.Should().Be("10:00");
        response.Hours.Last().From.Should().Be("12:00");
        response.Hours.Last().Until.Should().Be("13:00");
    }

    [Fact]
    public async Task RunAsync_DoesNotShowFollowingHour_IfSlotSpillsOver()
    {
        var slots = new[]
        {
            new SessionInstance(new DateTime(2077, 1, 1, 9, 55, 0), new DateTime(2077, 1, 1, 10, 5, 0)),
        };

        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.MultiServiceJointBookings))
            .ReturnsAsync(true);
        _siteService.Setup(x => x.GetSiteByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Site(
                "2de5bb57-060f-4cb5-b14d-16587d0c2e8f",
                "Test Site",
                "Test Address",
                "01234567890",
                "ODS1",
                "R1",
                "ICB1",
                string.Empty,
                new List<Accessibility>
                {
                    new("test_acces/one", "true")
                },
                new Location("Coords", [1.234, 5.678]),
                null,
                null,
                string.Empty));
        _bookingAvailabilityStateService.Setup(x => x.GetAvailableSlots(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(slots);
        _availableSlotsFilter.Setup(x => x.FilterAvailableSlots(It.IsAny<List<SessionInstance>>(), It.IsAny<List<Attendee>>()))
            .Returns(slots);

        var payload = new AvailabilityQueryByHoursRequest(
            "34e990af-5dc9-43a6-8895-b9123216d699",
            [
                new() { Services = ["RSV:Adult"]}
            ],
            new DateOnly(2077, 1, 1));

        var result = await _sut.RunAsync(CreateRequest(payload)) as ContentResult;

        result.StatusCode.Should().Be(200);

        var body = await new StringReader(result.Content).ReadToEndAsync();
        var response = JsonConvert.DeserializeObject<AvailabilityByHours>(body);

        response.Hours.Count.Should().Be(1);
        response.Hours.First().From.Should().Be("09:00");
        response.Hours.First().Until.Should().Be("10:00");
        response.Hours.Any(x => x.From == "10:00").Should().BeFalse();
    }

    [Fact]
    public async Task RunAsync_ReturnsEmptyHoursArray()
    {
        var slots = new List<SessionInstance>();
        
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.MultiServiceJointBookings))
            .ReturnsAsync(true);
        _siteService.Setup(x => x.GetSiteByIdAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Site(
                "2de5bb57-060f-4cb5-b14d-16587d0c2e8f",
                "Test Site",
                "Test Address",
                "01234567890",
                "ODS1",
                "R1",
                "ICB1",
                string.Empty,
                new List<Accessibility>
                {
                    new("test_acces/one", "true")
                },
                new Location("Coords", [1.234, 5.678]),
                null,
                null,
                string.Empty));
        _bookingAvailabilityStateService.Setup(x => x.GetAvailableSlots(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(slots);
        _availableSlotsFilter.Setup(x => x.FilterAvailableSlots(It.IsAny<List<SessionInstance>>(), It.IsAny<List<Attendee>>()))
            .Returns(slots);

        var payload = new AvailabilityQueryByHoursRequest(
            "34e990af-5dc9-43a6-8895-b9123216d699",
            [
                new() { Services = ["RSV:Adult"]}
            ],
            new DateOnly(2077, 1, 1));

        var result = await _sut.RunAsync(CreateRequest(payload)) as ContentResult;

        result.StatusCode.Should().Be(200);

        var body = await new StringReader(result.Content).ReadToEndAsync();
        var response = JsonConvert.DeserializeObject<AvailabilityByHours>(body);

        response.Hours.Count.Should().Be(0);
    }

    private static HttpRequest CreateRequest(AvailabilityQueryByHoursRequest payload)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        var body = JsonConvert.SerializeObject(payload);
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        request.Headers.Append("Authorization", "Test 123");
        return request;
    }
}
