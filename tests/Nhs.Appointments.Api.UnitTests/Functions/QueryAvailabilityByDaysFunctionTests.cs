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
public class QueryAvailabilityByDaysFunctionTests
{
    private readonly Mock<IBookingAvailabilityStateService> _bookingAvailabilityStateService = new();
    private readonly Mock<ILogger<QueryAvailabilityByDaysFunction>> _logger = new();
    private readonly Mock<IMetricsRecorder> _metricsRecorder = new();
    private readonly Mock<IUserContextProvider> _userContextProvider = new();
    private readonly Mock<IValidator<AvailabilityQueryRequest>> _validator = new();
    private readonly Mock<IFeatureToggleHelper> _featureToggleHelper = new();
    private readonly Mock<ISiteService> _siteService = new();
    private readonly Mock<IAvailableSlotsFilter> _availableSlotsFilter = new();

    private readonly QueryAvailabilityByDaysFunction _sut;

    public QueryAvailabilityByDaysFunctionTests()
    {
        _sut = new QueryAvailabilityByDaysFunction(
            _bookingAvailabilityStateService.Object,
            _validator.Object,
            _userContextProvider.Object,
            _logger.Object,
            _metricsRecorder.Object,
            _availableSlotsFilter.Object,
            _siteService.Object,
            _featureToggleHelper.Object);
        _validator.Setup(x => x.ValidateAsync(It.IsAny<AvailabilityQueryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    [Fact]
    public async Task RunAsync_ReturnsProblemResponse_WhenFeatureDisabled()
    {
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.MultiServiceJointBookings))
            .ReturnsAsync(false);

        var payload = new AvailabilityQueryRequest(
            ["test-site-123"],
            [
                new() { Services = ["RSV:Adult"]}
            ],
            new DateOnly(2025, 9, 1),
            new DateOnly(2025, 10, 1));

        var result = await _sut.RunAsync(CreatRequest(payload)) as ContentResult;

        result.StatusCode.Should().Be(501);
    }

    [Fact]
    public async Task RunAsync_ReturnsEmptyResponse_WhenAllSitesInactive()
    {
        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.MultiServiceJointBookings))
            .ReturnsAsync(true);
        _siteService.Setup(x => x.GetAllSites(It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new List<Site>
            {
                new(
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
                    true,
                    string.Empty
                    ),
                new(
                    "34e990af-5dc9-43a6-8895-b9123216d699",
                    "Test Site 2",
                    "Test Address 2",
                    "09876543210",
                    "ODS2",
                    "R2",
                    "ICB2",
                    string.Empty,
                    new List<Accessibility>
                    {
                        new("test_acces/one", "true")
                    },
                    new Location("Coords", [1.234, 5.678]),
                    null,
                    true,
                    string.Empty
                    )
            });

        var payload = new AvailabilityQueryRequest(
            ["test-site-123"],
            [
                new() { Services = ["RSV:Adult"]}
            ],
            new DateOnly(2025, 9, 1),
            new DateOnly(2025, 10, 1));

        var result = await _sut.RunAsync(CreatRequest(payload)) as ContentResult;

        result.StatusCode.Should().Be(200);

        var body = await new StringReader(result.Content).ReadToEndAsync();
        var response = JsonConvert.DeserializeObject<List<AvailabilityByDays>>(body);

        response.Count.Should().Be(0);

        _bookingAvailabilityStateService.Verify(b => b.GetAvailableSlots(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
        _availableSlotsFilter.Verify(a => a.FilterAvailableSlots(It.IsAny<List<SessionInstance>>(), It.IsAny<List<Attendee>>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_ReturnsSlots_SingleUser_SingleService()
    {
        var slots = new[]
        {
            new SessionInstance(new DateTime(2077, 1, 1, 9, 0, 0), new DateTime(2077, 1, 1, 9, 5, 0)),
            new SessionInstance(new DateTime(2077, 1, 2, 10, 0, 0), new DateTime(2077, 1, 2, 10, 5, 0)),
            new SessionInstance(new DateTime(2077, 1, 3, 11, 0, 0), new DateTime(2077, 1, 3, 11, 5, 0)),
        };

        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.MultiServiceJointBookings))
            .ReturnsAsync(true);
        _siteService.Setup(x => x.GetAllSites(It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new List<Site>
            {
                new(
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
                    string.Empty
                    )
            });
        _bookingAvailabilityStateService.Setup(x => x.GetAvailableSlots(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(slots);
        _availableSlotsFilter.Setup(x => x.FilterAvailableSlots(It.IsAny<List<SessionInstance>>(), It.IsAny<List<Attendee>>()))
            .Returns(slots);

        var payload = new AvailabilityQueryRequest(
            ["2de5bb57-060f-4cb5-b14d-16587d0c2e8f"],
            [
                new() { Services = ["RSV:Adult"]}
            ],
            new DateOnly(2077, 1, 1),
            new DateOnly(2077, 1, 3));

        var result = await _sut.RunAsync(CreatRequest(payload)) as ContentResult;

        result.StatusCode.Should().Be(200);

        var body = await new StringReader(result.Content).ReadToEndAsync();
        var response = JsonConvert.DeserializeObject<List<AvailabilityByDays>>(body);

        response.Count.Should().Be(1);
        response.First().Days.Count.Should().Be(3);
        response.First().Days.First().Blocks.Count.Should().Be(1);
    }

    [Fact]
    public async Task RunAsync_ReturnsSlots_TwoAttendees_SingleService()
    {
        var slots = new[]
        {
            new SessionInstance(new DateTime(2077, 1, 1, 9, 0, 0), new DateTime(2077, 1, 1, 9, 5, 0)),
            new SessionInstance(new DateTime(2077, 1, 2, 10, 0, 0), new DateTime(2077, 1, 2, 10, 5, 0)),
            new SessionInstance(new DateTime(2077, 1, 3, 11, 0, 0), new DateTime(2077, 1, 3, 11, 5, 0)),
        };

        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.MultiServiceJointBookings))
            .ReturnsAsync(true);
        _siteService.Setup(x => x.GetAllSites(It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new List<Site>
            {
                new(
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
                    string.Empty
                    )
            });
        _bookingAvailabilityStateService.Setup(x => x.GetAvailableSlots(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(slots);
        _availableSlotsFilter.Setup(x => x.FilterAvailableSlots(It.IsAny<List<SessionInstance>>(), It.IsAny<List<Attendee>>()))
            .Returns(slots);

        var payload = new AvailabilityQueryRequest(
            ["2de5bb57-060f-4cb5-b14d-16587d0c2e8f"],
            [
                new() { Services = ["RSV:Adult"]},
                new() { Services = ["RSV:Adult"]}
            ],
            new DateOnly(2077, 1, 1),
            new DateOnly(2077, 1, 3));

        var result = await _sut.RunAsync(CreatRequest(payload)) as ContentResult;

        result.StatusCode.Should().Be(200);

        var body = await new StringReader(result.Content).ReadToEndAsync();
        var response = JsonConvert.DeserializeObject<List<AvailabilityByDays>>(body);

        response.Count.Should().Be(1);
        response.First().Days.Count.Should().Be(3);
        response.First().Days.First().Blocks.Count.Should().Be(1);
    }

    [Fact]
    public async Task RunAsync_ReturnsSlots_TwoAttendees_MultipleServices()
    {
        var slots = new[]
        {
            new SessionInstance(new DateTime(2077, 1, 1, 9, 0, 0), new DateTime(2077, 1, 1, 9, 5, 0))
            {
                Services = ["RSV:Adult"]
            },
            new SessionInstance(new DateTime(2077, 1, 2, 10, 0, 0), new DateTime(2077, 1, 2, 10, 5, 0))
            {
                Services = ["RSV:Adult", "COVID:5_11"]
            },
            new SessionInstance(new DateTime(2077, 1, 3, 11, 0, 0), new DateTime(2077, 1, 3, 11, 5, 0))
            {
                Services = ["FLU:5_11"]
            }
        };

        _featureToggleHelper.Setup(x => x.IsFeatureEnabled(Flags.MultiServiceJointBookings))
            .ReturnsAsync(true);
        _siteService.Setup(x => x.GetAllSites(It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new List<Site>
            {
                new(
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
                    string.Empty
                    )
            });
        _bookingAvailabilityStateService.Setup(x => x.GetAvailableSlots(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(slots);
        _availableSlotsFilter.Setup(x => x.FilterAvailableSlots(It.IsAny<List<SessionInstance>>(), It.IsAny<List<Attendee>>()))
            .Returns(slots.Take(2));

        var payload = new AvailabilityQueryRequest(
            ["2de5bb57-060f-4cb5-b14d-16587d0c2e8f"],
            [
                new() { Services = ["RSV:Adult"]},
                new() { Services = ["COVID:5_11"]}
            ],
            new DateOnly(2077, 1, 1),
            new DateOnly(2077, 1, 3));

        var result = await _sut.RunAsync(CreatRequest(payload)) as ContentResult;

        result.StatusCode.Should().Be(200);

        var body = await new StringReader(result.Content).ReadToEndAsync();
        var response = JsonConvert.DeserializeObject<List<AvailabilityByDays>>(body);

        response.Count.Should().Be(1);
        response.First().Days.Count.Should().Be(2);
        response.First().Days.First().Blocks.Count.Should().Be(1);
    }

    private static HttpRequest CreatRequest(AvailabilityQueryRequest payload)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        var body = JsonConvert.SerializeObject(payload);
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        request.Headers.Append("Authorization", "Test 123");
        return request;
    }
}
