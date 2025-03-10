using Nhs.Appointments.Core.Concurrency;
using Nhs.Appointments.Core.Messaging;

namespace Nhs.Appointments.Core.UnitTests.AvailabilityCalculations;

public class AvailabilityCalculationsBase
{
    protected readonly AvailabilityService _sut;
    private readonly Mock<IAvailabilityStore> _availabilityStore = new();
    private readonly Mock<IAvailabilityCreatedEventStore> _availabilityCreatedEventStore = new();
    private readonly Mock<IBookingsService> _bookingsService = new();
    private readonly Mock<ISiteLeaseManager> _siteLeaseManager = new();
    private readonly Mock<IBookingsDocumentStore> _bookingsDocumentStore = new();
    private readonly Mock<IReferenceNumberProvider> _referenceNumberProvider = new();
    private readonly Mock<IBookingEventFactory> _eventFactory = new();
    private readonly Mock<IMessageBus> _messageBus = new();
    private readonly Mock<TimeProvider> _time = new();


    protected const string MockSite = "some-site";

    protected AvailabilityCalculationsBase() => _sut = new AvailabilityService(_availabilityStore.Object,
        _availabilityCreatedEventStore.Object, _bookingsService.Object, _siteLeaseManager.Object,
        _bookingsDocumentStore.Object, _referenceNumberProvider.Object, _eventFactory.Object, _messageBus.Object,
        _time.Object);

    private DateTime TestDateAt(string time)
    {
        var hour = int.Parse(time.Split(":")[0]);
        var minute = int.Parse(time.Split(":")[1]);
        return new DateTime(2025, 01, 01, hour, minute, 0);
    }

    protected Booking TestBooking(string reference, string service, string from = "09:00",
        int duration = 10, string avStatus = "Orphaned", string status = "Booked",
        int creationOrder = 1) =>
        new()
        {
            Reference = reference,
            Service = service,
            From = TestDateAt(from),
            Duration = duration,
            AvailabilityStatus = Enum.Parse<AvailabilityStatus>(avStatus),
            AttendeeDetails = new AttendeeDetails { FirstName = "Daniel", LastName = "Dixon" },
            Status = Enum.Parse<AppointmentStatus>(status),
            Created = new DateTime(2024, 11, 15, 9, 45, creationOrder)
        };

    protected SessionInstance TestSession(string start, string end, string[] services, int slotLength = 10,
        int capacity = 1) =>
        new(TestDateAt(start), TestDateAt(end)) { Services = services.ToList(), SlotLength = slotLength, Capacity = capacity };

    protected void SetupAvailabilityAndBookings(List<Booking> bookings, List<SessionInstance> sessions)
    {
        _bookingsService
            .Setup(x => x.GetBookings(It.IsAny<DateTime>(), It.IsAny<DateTime>(), MockSite))
            .ReturnsAsync(bookings);

        _availabilityStore
            .Setup(x => x.GetSessions(
                It.Is<string>(s => s == MockSite),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>()))
            .ReturnsAsync(sessions);
    }
}
