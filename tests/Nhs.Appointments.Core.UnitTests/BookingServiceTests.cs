using Nhs.Appointments.Core.Concurrency;
using Nhs.Appointments.Core.Messaging;

namespace Nhs.Appointments.Core.UnitTests
{
    public class BookingServiceTests
    {
        private readonly BookingsService _bookingsService;
        private readonly Mock<IBookingsDocumentStore> _bookingsDocumentStore = new();
        private readonly Mock<IReferenceNumberProvider> _referenceNumberProvider = new();
        private readonly Mock<ISiteLeaseManager> _siteLeaseManager = new();
        private readonly Mock<IAvailabilityCalculator> _availabilityCalculator = new();
        private readonly Mock<IMessageBus> _messageBus = new();

        public BookingServiceTests()
        {
            _bookingsService = new BookingsService(
                _bookingsDocumentStore.Object,
                _referenceNumberProvider.Object,
                _siteLeaseManager.Object,
                _availabilityCalculator.Object,
                _messageBus.Object);
        }

        [Fact (Skip = "flakey test. needs investigation")]
        public async Task MakeBooking_AcquiresLock_WhenBooking()
        {
            var expectedFrom = new DateOnly(2077, 1, 1);
            var expectedUntil = expectedFrom.AddDays(1);
            var availability = new[] { new SessionInstance(new DateTime(2077, 1, 1, 9, 0, 0, 0), new DateTime(2077, 1, 1, 12, 0, 0, 0)) };

            var booking = new Booking { Site = "TEST", Service = "TSERV", From = new DateTime(2077, 1, 1, 10, 0, 0, 0), Duration = 10 };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());
            _availabilityCalculator.Setup(x => x.CalculateAvailability("TEST", "TSERV", expectedFrom, expectedUntil)).ReturnsAsync(availability);
            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");

            var leaseManager = new FakeLeaseManager();
            var bookingService = new BookingsService(_bookingsDocumentStore.Object, _referenceNumberProvider.Object, leaseManager, _availabilityCalculator.Object, _messageBus.Object);
            
            var task = Task.Run(() => bookingService.MakeBooking(booking));
            var taskCompleted = task.Wait(100);
            taskCompleted.Should().BeFalse();           

            leaseManager.WaitHandle.Set();

            taskCompleted = task.Wait(100);
            taskCompleted.Should().BeTrue();            
        }

        [Fact]
        public async Task MakeBooking_CallsAvailabilityCalculator_WhenBooking()
        {
            var expectedFrom = new DateOnly(2077, 1, 1);
            var expectedUntil = expectedFrom.AddDays(1);

            var booking = new Booking { Site = "TEST", Service = "TSERV", From = new DateTime(2077, 1, 1) };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());
            var result = await _bookingsService.MakeBooking(booking);

            _availabilityCalculator.Verify(x => x.CalculateAvailability("TEST", "TSERV", expectedFrom, expectedUntil));
        }

        [Fact]
        public async Task MakeBooking_ReturnsSuccess_WhenSlotIsAvailable()
        {
            var expectedFrom = new DateOnly(2077, 1, 1);
            var expectedUntil = expectedFrom.AddDays(1);
            var availability = new[] { new SessionInstance(new DateTime(2077, 1, 1, 9, 0, 0, 0), new DateTime(2077, 1, 1, 12, 0, 0, 0)) };

            var booking = new Booking { Site = "TEST", Service = "TSERV", From = new DateTime(2077, 1, 1, 10, 0, 0, 0), Duration = 10, ContactDetails = [new ContactItem()], AttendeeDetails = new AttendeeDetails()  };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());
            _availabilityCalculator.Setup(x => x.CalculateAvailability("TEST", "TSERV", expectedFrom, expectedUntil)).ReturnsAsync(availability);
            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");

            var result = await _bookingsService.MakeBooking(booking);
            result.Success.Should().BeTrue();
            result.Reference.Should().Be("TEST1");            
        }

        [Fact]
        public async Task MakeBooking_ReturnsNonSuccess_WhenSlotIsNotAvailable()
        {
            var expectedFrom = new DateOnly(2077, 1, 1);
            var expectedUntil = expectedFrom.AddDays(1);
            var availability = new[] { new SessionInstance(new DateTime(2077, 1, 1, 9, 0, 0, 0), new DateTime(2077, 1, 1, 12, 0, 0, 0)) };

            var booking = new Booking { Site = "TEST", Service = "TSERV", From = new DateTime(2077, 1, 1, 13, 0, 0, 0), Duration = 10 };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());
            _availabilityCalculator.Setup(x => x.CalculateAvailability("TEST", "TSERV", expectedFrom, expectedUntil)).ReturnsAsync(availability);
            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");

            var result = await _bookingsService.MakeBooking(booking);
            result.Success.Should().BeFalse();
            result.Reference.Should().Be(string.Empty);
        }

    }

    public class FakeLeaseManager : ISiteLeaseManager
    {
        private readonly AutoResetEvent _waitHandle;

        public FakeLeaseManager()
        {
            _waitHandle = new AutoResetEvent(false);
        }



        public AutoResetEvent WaitHandle => _waitHandle;

        public ISiteLeaseContext Acquire(string site)
        {
            _waitHandle.WaitOne();
            return new FakeLeaseContext();
        }
    }

    public class FakeLeaseContext : ISiteLeaseContext
    {
        public void Dispose()
        {
            
        }
    }
}
