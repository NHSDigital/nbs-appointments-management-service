namespace Nhs.Appointments.Core.UnitTests.BookingAvailabilityState;

public class BookingAvailabilityStateServiceTestBase
{
    protected readonly BookingAvailabilityStateService Sut;
    protected readonly Mock<TimeProvider> TimeProvider = new();
    private readonly Mock<IBookingsDocumentStore> _bookingsDocumentStore = new();
    private readonly Mock<IAvailabilityStore> _availabilityStore = new();
    private readonly Mock<IAvailabilityCreatedEventStore> _availabilityCreatedEventStore = new();
    
    protected const string MockSite = "some-site";

    protected BookingAvailabilityStateServiceTestBase() => Sut =
        new BookingAvailabilityStateService(new AvailabilityQueryService(_availabilityStore.Object, _availabilityCreatedEventStore.Object), new BookingQueryService(_bookingsDocumentStore.Object, TimeProvider.Object));

    private static DateTime TestDateAt(string time)
    {
        var hour = int.Parse(time.Split(":")[0]);
        var minute = int.Parse(time.Split(":")[1]);
        return new DateTime(2025, 01, 01, hour, minute, 0);
    }
    
    private static DateTime TestDateAt(DateOnly date, string time)
    {
        var hour = int.Parse(time.Split(":")[0]);
        var minute = int.Parse(time.Split(":")[1]);
        
        return new DateTime(date.Year, date.Month, date.Day, hour, minute, 0);
    }

    protected static Booking TestBooking(string reference, string service, string from = "09:00",
        int duration = 10, string avStatus = "Orphaned", string status = "Booked",
        int creationOrder = 1, DateTime? creationDate = null) =>
        new()
        {
            Reference = reference,
            Service = service,
            From = TestDateAt(from),
            Duration = duration,
            AvailabilityStatus = Enum.Parse<AvailabilityStatus>(avStatus),
            AttendeeDetails = new AttendeeDetails { FirstName = "Daniel", LastName = "Dixon" },
            Status = Enum.Parse<AppointmentStatus>(status),
            Created = creationDate ?? new DateTime(2024, 11, 15, 9, 45, creationOrder)
        };
    
    protected static Booking TestBooking(string reference, string service, DateOnly date, string from = "09:00",
        int duration = 10, string avStatus = "Orphaned", string status = "Booked",
        int creationOrder = 1, DateTime? creationDate = null) =>
        new()
        {
            Reference = reference,
            Service = service,
            From = TestDateAt(date, from),
            Duration = duration,
            AvailabilityStatus = Enum.Parse<AvailabilityStatus>(avStatus),
            AttendeeDetails = new AttendeeDetails { FirstName = "Daniel", LastName = "Dixon" },
            Status = Enum.Parse<AppointmentStatus>(status),
            Created = creationDate ?? new DateTime(2024, 11, 15, 9, 45, creationOrder)
        };

    protected static SessionInstance TestSession(string start, string end, string[] services, int slotLength = 10,
        int capacity = 1) =>
        new(TestDateAt(start), TestDateAt(end)) { Services = services, SlotLength = slotLength, Capacity = capacity };
    
    protected static SessionInstance TestSession(DateOnly date, string start, string end, string[] services, int slotLength = 10,
        int capacity = 1, Guid? internalSessionId = null) =>
        new(TestDateAt(date, start), TestDateAt(date, end)) { InternalSessionId = internalSessionId, Services = services, SlotLength = slotLength, Capacity = capacity };

    protected void SetupAvailabilityAndBookings(List<Booking> bookings, List<SessionInstance> sessions)
    {
        _bookingsDocumentStore.Setup(x => x.GetInDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), MockSite))
            .ReturnsAsync(bookings);

        _availabilityStore
            .Setup(x => x.GetSessions(
                It.Is<string>(s => s == MockSite),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<bool>()))
            .ReturnsAsync(sessions);
    }
}
