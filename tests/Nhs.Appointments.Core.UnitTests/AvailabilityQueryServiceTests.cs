using Nhs.Appointments.Core.Features;

namespace Nhs.Appointments.Core.UnitTests;

public class AvailabilityQueryServiceTests
{
    private readonly Mock<IAvailabilityCreatedEventStore> _availabilityCreatedEventStore = new();
    private readonly Mock<IAvailabilityStore> _availabilityStore = new();
    private readonly AvailabilityQueryService _sut;

    public AvailabilityQueryServiceTests()  => _sut = new AvailabilityQueryService(_availabilityStore.Object, _availabilityCreatedEventStore.Object);

    [Fact]
    public async Task GetAvailabilityCreatedEvents_OrdersEventsByFromThenByTo()
    {
        var availabilityCreatedEvents = new List<AvailabilityCreatedEvent>
        {
            new()
            {
                Created = DateTime.UtcNow,
                By = "some.user@nhs.net",
                Site = "some-site",
                From = DateOnly.FromDateTime(new DateTime(2025, 4, 3)),
            },
            new()
            {
                Created = DateTime.UtcNow,
                By = "some.user@nhs.net",
                Site = "some-site",
                From = DateOnly.FromDateTime(new DateTime(2024, 10, 10)),
                To = DateOnly.FromDateTime(new DateTime(2024, 10, 20)),
            },
            new()
            {
                Created = DateTime.UtcNow,
                By = "some.user@nhs.net",
                Site = "some-site",
                From = DateOnly.FromDateTime(new DateTime(2024, 10, 10)),
                To = DateOnly.FromDateTime(new DateTime(2024, 10, 15)),
            }
        };

        _availabilityCreatedEventStore.Setup(x => x.GetAvailabilityCreatedEvents(It.IsAny<string>()))
            .ReturnsAsync(availabilityCreatedEvents);

        var fromDate = DateOnly.FromDateTime(DateTime.MinValue);
        var result = (await _sut.GetAvailabilityCreatedEventsAsync("some-site", fromDate)).ToList();

        result.Should().HaveCount(3);

        result[0].From.Should().Be(DateOnly.FromDateTime(new DateTime(2024, 10, 10)));
        result[0].To.Should().Be(DateOnly.FromDateTime(new DateTime(2024, 10, 15)));

        result[1].From.Should().Be(DateOnly.FromDateTime(new DateTime(2024, 10, 10)));
        result[1].To.Should().Be(DateOnly.FromDateTime(new DateTime(2024, 10, 20)));

        result[2].From.Should().Be(DateOnly.FromDateTime(new DateTime(2025, 4, 3)));
        result[2].To.Should().BeNull();
    }

    [Fact]
    public async Task GetAvailabilityCreatedEvents_FiltersEventsAfterDate()
    {
        var availabilityCreatedEvents = new List<AvailabilityCreatedEvent>
        {
            new()
            {
                Created = DateTime.UtcNow,
                By = "some.user@nhs.net",
                Site = "some-site",
                From = DateOnly.FromDateTime(new DateTime(2025, 4, 3)),
            },
            new()
            {
                Created = DateTime.UtcNow,
                By = "some.user@nhs.net",
                Site = "some-site",
                From = DateOnly.FromDateTime(new DateTime(2024, 10, 10)),
                To = DateOnly.FromDateTime(new DateTime(2024, 10, 20)),
            },
            new()
            {
                Created = DateTime.UtcNow,
                By = "some.user@nhs.net",
                Site = "some-site",
                From = DateOnly.FromDateTime(new DateTime(2024, 10, 10)),
                To = DateOnly.FromDateTime(new DateTime(2024, 10, 15)),
            }
        };

        _availabilityCreatedEventStore.Setup(x => x.GetAvailabilityCreatedEvents(It.IsAny<string>()))
            .ReturnsAsync(availabilityCreatedEvents);

        var fromDate = DateOnly.FromDateTime(new DateTime(2024, 10, 17));
        var result = (await _sut.GetAvailabilityCreatedEventsAsync("some-site", fromDate)).ToList();

        result.Should().HaveCount(2);

        result[0].From.Should().Be(DateOnly.FromDateTime(new DateTime(2024, 10, 10)));
        result[0].To.Should().Be(DateOnly.FromDateTime(new DateTime(2024, 10, 20)));

        result[1].From.Should().Be(DateOnly.FromDateTime(new DateTime(2025, 4, 3)));
        result[1].To.Should().BeNull();
    }

    [Fact]
    public async Task GetDailyAvailabiltiy_ReturnsAvailabilityWithinDateRange()
    {
        var fromDate = DateOnly.FromDateTime(new DateTime(2024, 12, 1));
        var toDate = DateOnly.FromDateTime(new DateTime(2024, 12, 8));

        var availability = new List<DailyAvailability>
        {
            new()
            {
                Date = DateOnly.FromDateTime(new DateTime(2024, 12, 1)),
                Sessions =
                [
                    new()
                    {
                        From = TimeOnly.FromTimeSpan(TimeSpan.FromHours(11)),
                        Until = TimeOnly.FromTimeSpan(TimeSpan.FromHours(16)),
                        Capacity = 2,
                        SlotLength = 5,
                        Services = ["RSV:Adult"]
                    }
                ]
            },
            new()
            {
                Date = DateOnly.FromDateTime(new DateTime(2024, 12, 4)),
                Sessions =
                [
                    new()
                    {
                        From = TimeOnly.FromTimeSpan(TimeSpan.FromHours(11)),
                        Until = TimeOnly.FromTimeSpan(TimeSpan.FromHours(16)),
                        Capacity = 2,
                        SlotLength = 5,
                        Services = ["RSV:Adult"]
                    }
                ]
            }
        };

        _availabilityStore.Setup(x =>
                x.GetDailyAvailability(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(availability);

        var result = await _sut.GetDailyAvailability("TEST01", fromDate, toDate);

        result.Any().Should().BeTrue();
        result.Count().Should().Be(2);
    }
}
