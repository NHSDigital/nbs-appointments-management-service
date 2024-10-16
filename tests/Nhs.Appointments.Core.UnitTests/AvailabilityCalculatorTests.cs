namespace Nhs.Appointments.Core.UnitTests;

public class AvailabilityCalculatorTests
{
    private readonly AvailabilityCalculator _sut;
    private readonly Mock<IBookingsDocumentStore> _bookingDocumentStore = new();
    private readonly Mock<IScheduleService> _scheduleService = new();

    public AvailabilityCalculatorTests()
    {
        _sut = new AvailabilityCalculator(_scheduleService.Object, _bookingDocumentStore.Object);
    }

    [Fact]
    public async Task CalculateAvailability_ReturnsSessionInstancesForAllHolders_WhenMultipleSessionHolders()
    {
        var site = "1";
        var service = "COVID";
        var requestFrom = new DateOnly(2077, 1, 1);
        var requestUntil = new DateOnly(2077, 1, 4);
        var sessions = new List<SessionInstance>()
        {
            CreateSessionInstance("holder1", new DateTime(2077, 1, 1, 9, 0, 0), new DateTime(2077, 1, 1, 12, 0, 0)),
            CreateSessionInstance("holder1", new DateTime(2077, 1, 2, 9, 0, 0), new DateTime(2077, 1, 2, 12, 0, 0)),
            CreateSessionInstance("holder2", new DateTime(2077, 1, 3, 9, 0, 0), new DateTime(2077, 1, 3, 12, 0, 0)),
            CreateSessionInstance("holder2", new DateTime(2077, 1, 4, 9, 0, 0), new DateTime(2077, 1, 4, 12, 0, 0))
        };
        _scheduleService.Setup(x => x.GetSessions(site, service, requestFrom, requestUntil))
            .ReturnsAsync(sessions);
        var result = await _sut.CalculateAvailability(site, service , requestFrom, requestUntil);

        Assert.NotNull(result);
        Assert.Equal(4, result.Count());
    }

    [Fact]
    public async Task AvailabilityCalculator_SplitsBlocks_ForSessionsWithExistingBookings()
    {
        var site = "1000";
        var service = "COVID";
        var sessionFromDateTime = new DateTime(2077, 1, 1, 9, 0, 0);
        var sessionUntilDateTime = new DateTime(2077, 1, 1, 10, 0, 0);
        var appointmentDateAndTime = new DateTime(2077, 1, 1, 9, 40, 0);
        var appointmentDuration = 5;
        var sessions = new List<SessionInstance>()
        {
            CreateSessionInstance("SessionHolder", sessionFromDateTime, sessionUntilDateTime),
        };
        var booking = new List<Booking>
        {
            CreateTestBooking(appointmentDateAndTime, appointmentDuration, service, site)
        };
        var expectedResult = new List<SessionInstance>
        {
            new (new DateTime(2077, 1, 1, 9,0,0), new DateTime(2077, 1, 1, 9,40,0)){SessionHolder = "SessionHolder"},
            new (new DateTime(2077, 1, 1, 9, 45, 0), new DateTime(2077, 1, 1,10,0,0)){SessionHolder = "SessionHolder"},
        };
        _scheduleService.Setup(x => x.GetSessions(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(sessions);
        _bookingDocumentStore.Setup(x => x.GetInDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>())).ReturnsAsync(booking);

        var result = await _sut.CalculateAvailability(site, service, DateOnly.FromDateTime(sessionFromDateTime) , DateOnly.FromDateTime(sessionUntilDateTime));
        result.Should().BeEquivalentTo(expectedResult);
    }
    
    [Fact]
    public async Task AvailabilityCalculator_OnlySplitsBlocks_ForSessionsThatHaveBookings()
    {
        var site = "1000";
        var service = "COVID";
        var sessionHolderOne = "SessionHolder-One";
        var sessionHolderTwo = "SessionHolder-Two";
        var sessionFromDateTime = new DateTime(2077, 1, 1, 9, 0, 0);
        var sessionUntilDateTime = new DateTime(2077, 1, 1, 10, 0, 0);
        var appointmentDateAndTime = new DateTime(2077, 1, 1, 9, 40, 0);
        var appointmentDuration = 5;
        var sessions = new List<SessionInstance>()
        {
            CreateSessionInstance(sessionHolderOne, sessionFromDateTime, sessionUntilDateTime),
            CreateSessionInstance(sessionHolderTwo, sessionFromDateTime, sessionUntilDateTime),
        };
        var booking = new List<Booking>
        {
            CreateTestBooking(appointmentDateAndTime, appointmentDuration, service, site, sessionHolderOne)
        };
        var expectedResult = new List<SessionInstance>
        {
            new (new DateTime(2077, 1, 1, 9,0,0), new DateTime(2077, 1, 1, 9,40,0)){SessionHolder = sessionHolderOne},
            new (new DateTime(2077, 1, 1, 9, 45, 0), new DateTime(2077, 1, 1, 10,0,0)){SessionHolder = sessionHolderOne},
            new (new DateTime(2077, 1, 1, 9,0,0), new DateTime(2077, 1, 1, 10,0,0)){SessionHolder = sessionHolderTwo}
        };
        _scheduleService.Setup(x => x.GetSessions(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(sessions);
        _bookingDocumentStore.Setup(x => x.GetInDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>())).ReturnsAsync(booking);

        var result = await _sut.CalculateAvailability(site, service, DateOnly.FromDateTime(sessionFromDateTime) , DateOnly.FromDateTime(sessionUntilDateTime));
        result.Should().BeEquivalentTo(expectedResult);
    }

    [Fact]
    public async Task AvailabilityCalculator_ReturnsEmptySessionInstance_WhenNoSessionsWereFound()
    {
        var site = "1000";
        var service = "COVID";
        var from = new DateOnly(2077, 1, 1);
        var until = new DateOnly(2077, 1, 1);
        var sessions = new List<SessionInstance>();
        _scheduleService.Setup(x => x.GetSessions(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(sessions);

        var result = await _sut.CalculateAvailability(site, service, from , until);
        result.Should().NotBeNull();
        result.Count().Should().Be(0);
    }

    private static SessionInstance CreateSessionInstance(string holder, DateTime from, DateTime until)
    {
        var sessionInstance = new SessionInstance(from, until);
        sessionInstance.SessionHolder = holder;
        return sessionInstance;
    }
    
    private static Booking CreateTestBooking(DateTime appointmentDateAndTime, int appointmentDuration, string service, string site, string sessionHolder = "SessionHolder")
    {
        var testBooking = new Booking
        {
            Reference = "123",
            From = appointmentDateAndTime,
            Duration = appointmentDuration,
            Service = service,
            Site = site,
            SessionHolder = sessionHolder,
            Outcome = null,
            AttendeeDetails = new AttendeeDetails()
            {
                NhsNumber = "999999999",
                DateOfBirth = new DateOnly(2000, 01, 01),
                FirstName = "FirstName",
                LastName = "LastName"
            }
        };
        return testBooking;
    }
}