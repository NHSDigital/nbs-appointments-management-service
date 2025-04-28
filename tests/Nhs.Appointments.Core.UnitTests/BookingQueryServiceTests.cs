namespace Nhs.Appointments.Core.UnitTests;

public class BookingQueryServiceTests
{
    private readonly BookingQueryService _bookingQueryService;
    private readonly Mock<IBookingsDocumentStore> _bookingsDocumentStore = new();

    public BookingQueryServiceTests()
    {
        _bookingQueryService = new BookingQueryService(_bookingsDocumentStore.Object, TimeProvider.System);
    }

    [Fact]
    public async Task GetBookings_ReturnsOrderedBookingsForSite()
    {
        var site = "some-site";
        IEnumerable<Booking> bookings = new List<Booking>
        {
            new Booking
            {
                From = new DateTime(2025, 01, 01, 16, 0, 0),
                Reference = "1",
                AttendeeDetails = new AttendeeDetails { FirstName = "Daniel", LastName = "Dixon" },
            },
            new Booking
            {
                From = new DateTime(2025, 01, 01, 14, 0, 0),
                Reference = "2",
                AttendeeDetails = new AttendeeDetails { FirstName = "Alexander", LastName = "Cooper" },
            },
            new Booking
            {
                From = new DateTime(2025, 01, 01, 14, 0, 0),
                Reference = "3",
                AttendeeDetails = new AttendeeDetails { FirstName = "Alexander", LastName = "Brown" },
            },
            new Booking
            {
                From = new DateTime(2025, 01, 01, 10, 0, 0),
                Reference = "4",
                AttendeeDetails = new AttendeeDetails { FirstName = "Bob", LastName = "Dawson" },
            }
        };

        _bookingsDocumentStore
            .Setup(x => x.GetInDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), site, It.IsAny<string>()))
            .ReturnsAsync(bookings);

        var response = await _bookingQueryService.GetBookings(new DateTime(), new DateTime(), site, It.IsAny<string>());

        Assert.Multiple(
            () => Assert.True(response.ToArray()[0].Reference == "4"),
            () => Assert.True(response.ToArray()[1].Reference == "3"),
            () => Assert.True(response.ToArray()[2].Reference == "2"),
            () => Assert.True(response.ToArray()[3].Reference == "1")
        );
    }
}
