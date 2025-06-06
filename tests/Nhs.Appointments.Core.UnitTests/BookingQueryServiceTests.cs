namespace Nhs.Appointments.Core.UnitTests;

public class BookingQueryServiceTests
{
    private readonly BookingQueryService _bookingQueryService;
    private readonly Mock<IBookingsDocumentStore> _bookingsDocumentStore = new();
    private readonly Mock<TimeProvider> _timeProvider = new();

    public BookingQueryServiceTests()
    {
        _bookingQueryService = new BookingQueryService(_bookingsDocumentStore.Object, _timeProvider.Object);
    }

    [Fact]
    public async Task GetBookings_ReturnsOrderedBookingsForSite()
    {
        var site = "some-site";
        IEnumerable<Booking> bookings = new List<Booking>
        {
            new Booking
            {
                Created = new DateTime(2025, 01, 01, 09, 0, 0),
                From = new DateTime(2025, 01, 01, 16, 0, 0),
                Reference = "1",
                AttendeeDetails = new AttendeeDetails { FirstName = "Daniel", LastName = "Dixon" },
            },
            new Booking
            {
                Created = new DateTime(2025, 01, 01, 09, 0, 0),
                From = new DateTime(2025, 01, 01, 14, 0, 0),
                Reference = "2",
                AttendeeDetails = new AttendeeDetails { FirstName = "Alexander", LastName = "Cooper" },
            },
            new Booking
            {
                Created = new DateTime(2025, 01, 01, 09, 0, 0),
                From = new DateTime(2025, 01, 01, 14, 0, 0),
                Reference = "3",
                AttendeeDetails = new AttendeeDetails { FirstName = "Alexander", LastName = "Brown" },
            },
            new Booking
            {
                Created = new DateTime(2025, 01, 01, 09, 0, 0),
                From = new DateTime(2025, 01, 01, 10, 0, 0),
                Reference = "4",
                AttendeeDetails = new AttendeeDetails { FirstName = "Bob", LastName = "Dawson" },
            }
        };

        _bookingsDocumentStore
            .Setup(x => x.GetInDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), site))
            .ReturnsAsync(bookings);

        var response = await _bookingQueryService.GetBookings(new DateTime(), new DateTime(), site);

        Assert.Multiple(
            () => Assert.Equal("4", response.ToArray()[0].Reference),
            () => Assert.Equal("3", response.ToArray()[1].Reference),
            () => Assert.Equal("2", response.ToArray()[2].Reference),
            () => Assert.Equal("1", response.ToArray()[3].Reference));
    }

    [Fact]
    public async Task GetOrderedLiveBookings_ExcludesUnknownAndCancelledBookings()
    {
        var date = DateTimeOffset.ParseExact("2025-01-01 00:00", "yyyy-MM-dd HH:mm", null);
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(date);

        var site = "some-site";
        IEnumerable<Booking> bookings = new List<Booking>
        {
            new Booking
            {
                Created = new DateTime(2025, 01, 01, 09, 0, 0),
                From = new DateTime(2025, 01, 01, 16, 0, 0),
                Reference = "1",
                Status = AppointmentStatus.Booked,
                AttendeeDetails = new AttendeeDetails { FirstName = "Daniel", LastName = "Dixon" },
            },
            new Booking
            {
                Created = new DateTime(2025, 01, 01, 09, 0, 0),
                From = new DateTime(2025, 01, 01, 14, 0, 0),
                Reference = "2",
                Status = AppointmentStatus.Unknown,
                AttendeeDetails = new AttendeeDetails { FirstName = "Alexander", LastName = "Cooper" },
            },
            new Booking
            {
                Created = new DateTime(2025, 01, 01, 09, 0, 0),
                From = new DateTime(2025, 01, 01, 14, 0, 0),
                Reference = "3",
                Status = AppointmentStatus.Provisional,
                AttendeeDetails = new AttendeeDetails { FirstName = "Alexander", LastName = "Brown" },
            },
            new Booking
            {
                Created = new DateTime(2025, 01, 01, 09, 0, 0),
                From = new DateTime(2025, 01, 01, 10, 0, 0),
                Reference = "4",
                Status = AppointmentStatus.Cancelled,
                AttendeeDetails = new AttendeeDetails { FirstName = "Bob", LastName = "Dawson" },
            }
        };

        _bookingsDocumentStore
            .Setup(x => x.GetInDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), site))
            .ReturnsAsync(bookings);

        var response = await _bookingQueryService.GetOrderedLiveBookings(site, new DateTime(), new DateTime());

        response.Should().HaveCount(2);

        Assert.Multiple(
            () => Assert.Equal("3", response.ToArray()[0].Reference),
            () => Assert.Equal("1", response.ToArray()[1].Reference));
    }

    [Fact]
    public async Task GetOrderedLiveBookings_ExcludesExpiredProvisionalBookings()
    {
        var date = DateTimeOffset.ParseExact("2025-01-01 09:00", "yyyy-MM-dd HH:mm", null);
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(date);

        var site = "some-site";
        IEnumerable<Booking> bookings = new List<Booking>
        {
            new Booking
            {
                Created = new DateTime(2025, 01, 01, 09, 0, 0),
                From = new DateTime(2025, 01, 01, 16, 0, 0),
                Reference = "1",
                Status = AppointmentStatus.Booked,
                AttendeeDetails = new AttendeeDetails { FirstName = "Daniel", LastName = "Dixon" },
            },
            new Booking
            {
                Created = new DateTime(2025, 01, 01, 09, 0, 0),
                From = new DateTime(2025, 01, 01, 14, 0, 0),
                Reference = "2",
                Status = AppointmentStatus.Unknown,
                AttendeeDetails = new AttendeeDetails { FirstName = "Alexander", LastName = "Cooper" },
            },
            new Booking
            {
                Created = new DateTime(2025, 01, 01, 08, 50, 0),
                From = new DateTime(2025, 01, 01, 14, 0, 0),
                Reference = "3",
                Status = AppointmentStatus.Provisional,
                AttendeeDetails = new AttendeeDetails { FirstName = "Alexander", LastName = "Brown" },
            },
            new Booking
            {
                Created = new DateTime(2025, 01, 01, 08, 54, 0),
                From = new DateTime(2025, 01, 01, 14, 0, 0),
                Reference = "4",
                Status = AppointmentStatus.Provisional,
                AttendeeDetails = new AttendeeDetails { FirstName = "Alexander", LastName = "Brown" },
            },
            new Booking
            {
                Created = new DateTime(2025, 01, 01, 08, 55, 0),
                From = new DateTime(2025, 01, 01, 14, 0, 0),
                Reference = "5",
                Status = AppointmentStatus.Provisional,
                AttendeeDetails = new AttendeeDetails { FirstName = "Alexander", LastName = "Brown" },
            },
            new Booking
            {
                Created = new DateTime(2025, 01, 01, 08, 55, 1),
                From = new DateTime(2025, 01, 01, 14, 0, 0),
                Reference = "6",
                Status = AppointmentStatus.Provisional,
                AttendeeDetails = new AttendeeDetails { FirstName = "Alexander", LastName = "Brown" },
            },
            new Booking
            {
                Created = new DateTime(2025, 01, 01, 08, 54, 59),
                From = new DateTime(2025, 01, 01, 14, 0, 0),
                Reference = "7",
                Status = AppointmentStatus.Provisional,
                AttendeeDetails = new AttendeeDetails { FirstName = "Alexander", LastName = "Brown" },
            },
        };

        _bookingsDocumentStore
            .Setup(x => x.GetInDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), site))
            .ReturnsAsync(bookings);

        var response = await _bookingQueryService.GetOrderedLiveBookings(site, new DateTime(), new DateTime());

        response.Should().HaveCount(3);

        Assert.Multiple(
            () => Assert.Equal("5", response.ToArray()[0].Reference),
            () => Assert.Equal("6", response.ToArray()[1].Reference),
            () => Assert.Equal("1", response.ToArray()[2].Reference));
    }
}
