using Nhs.Appointments.Core.Concurrency;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;

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
                new EventFactory(),
                _messageBus.Object,
                TimeProvider.System);
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
            var bookingService = new BookingsService(_bookingsDocumentStore.Object, _referenceNumberProvider.Object, leaseManager, _availabilityCalculator.Object, new EventFactory(), _messageBus.Object, TimeProvider.System);
            
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
        public async Task MakeBooking_RaisesNotificationEvent_WhenBooking()
        {
            var expectedFrom = new DateOnly(2077, 1, 1);
            var expectedUntil = expectedFrom.AddDays(1);
            var availability = new[] { new SessionInstance(new DateTime(2077, 1, 1, 10, 0, 0, 0), new DateTime(2077, 1, 1, 10, 10, 0, 0)) { Services = new[] { "TSERV" } } };

            var booking = new Booking { Site = "TEST", Service = "TSERV", From = new DateTime(2077, 1, 1, 10, 0, 0, 0), Duration = 10, ContactDetails = [new ContactItem()], AttendeeDetails = new AttendeeDetails() };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());
            _availabilityCalculator.Setup(x => x.CalculateAvailability("TEST", "TSERV", expectedFrom, expectedUntil)).ReturnsAsync(availability);
            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");

            var result = await _bookingsService.MakeBooking(booking);

            _messageBus.Verify(x => x.Send(It.Is<BookingMade>(e => e.Reference == booking.Reference)));
        }

        [Fact]
        public async Task MakeBooking_DoesNotRaiseNotificationEvent_WhenProvisional()
        {
            var expectedFrom = new DateOnly(2077, 1, 1);
            var expectedUntil = expectedFrom.AddDays(1);
            var availability = new[] { new SessionInstance(new DateTime(2077, 1, 1, 10, 0, 0, 0), new DateTime(2077, 1, 1, 10, 10, 0, 0)) { Services = new[] { "TSERV" } } };

            var booking = new Booking { Site = "TEST", Service = "TSERV", From = new DateTime(2077, 1, 1, 10, 0, 0, 0), Duration = 10, ContactDetails = null, Provisional = true };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());
            _availabilityCalculator.Setup(x => x.CalculateAvailability("TEST", "TSERV", expectedFrom, expectedUntil)).ReturnsAsync(availability);
            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");

            var result = await _bookingsService.MakeBooking(booking);

            _messageBus.Verify(x => x.Send(It.Is<BookingMade>(e => e.Reference == booking.Reference)), Times.Never);
        }

        [Fact]
        public async Task RescheduleBooking_IsSuccessful()
        {
            var expectedFrom = new DateOnly(2077, 1, 1);
            var expectedUntil = expectedFrom.AddDays(1);
            var availability = new[] {
                new SessionInstance(new DateTime(2077, 1, 1, 10, 0, 0, 0), new DateTime(2077, 1, 1, 10, 10, 0, 0))
                { Services = new[] { "TSERV" } },
                new SessionInstance(new DateTime(2077, 1, 1, 11, 0, 0, 0), new DateTime(2077, 1, 1, 11, 10, 0, 0))
                { Services = new[] { "TSERV" } },
            };

            ContactItem[] contactDetails = [new ContactItem { Type = "email", Value = "test@tempuri.org" }];

            var initialBooking = new Booking { Site = "TEST", Service = "TSERV", From = new DateTime(2077, 1, 1, 10, 0, 0, 0), Duration = 10, ContactDetails = null, Provisional = true };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());
            _availabilityCalculator.Setup(x => x.CalculateAvailability("TEST", "TSERV", expectedFrom, expectedUntil)).ReturnsAsync(availability);
            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");
            _bookingsDocumentStore.Setup(x => x.ConfirmProvisional(It.IsAny<string>(), It.IsAny<IEnumerable<ContactItem>>(), It.IsAny<string>())).ReturnsAsync(BookingConfirmationResult.Success);
            _bookingsDocumentStore.Setup(x => x.GetByReferenceOrDefaultAsync("TEST1")).ReturnsAsync(new Booking { Reference = "TEST1", Site = "TEST", Service = "TSERV", From = new DateTime(2077, 1, 1, 10, 0, 0, 0), Duration = 10, ContactDetails = contactDetails, Provisional = false, AttendeeDetails = new AttendeeDetails() });
            _bookingsDocumentStore.Setup(x => x.GetByReferenceOrDefaultAsync("TEST2")).ReturnsAsync(new Booking { Reference = "TEST2", Site = "TEST", Service = "TSERV", From = new DateTime(2077, 1, 1, 11, 0, 0, 0), Duration = 10, ContactDetails = contactDetails, Provisional = false, AttendeeDetails = new AttendeeDetails() });
            var initialBookingResult = await _bookingsService.MakeBooking(initialBooking);
            await _bookingsService.ConfirmProvisionalBooking(initialBookingResult.Reference, contactDetails, "");

            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST2");

            var rescheduledBooking = new Booking { Site = "TEST", Service = "TSERV", From = new DateTime(2077, 1, 1, 11, 0, 0, 0), Duration = 10, ContactDetails = null, Provisional = true };
            var rescheduledBookingResult = await _bookingsService.MakeBooking(rescheduledBooking);

            var rescheduleResult = await _bookingsService.ConfirmProvisionalBooking(rescheduledBooking.Reference, contactDetails, initialBookingResult.Reference);

            rescheduleResult.Should().Be(BookingConfirmationResult.Success);
        }

        [Fact]
        public async Task RescheduleBooking_RaisesNotificationEvent()
        {
            var expectedFrom = new DateOnly(2077, 1, 1);
            var expectedUntil = expectedFrom.AddDays(1);
            var availability = new[] {
                new SessionInstance(new DateTime(2077, 1, 1, 10, 0, 0, 0), new DateTime(2077, 1, 1, 10, 10, 0, 0))
                { Services = new[] { "TSERV" } },
                new SessionInstance(new DateTime(2077, 1, 1, 11, 0, 0, 0), new DateTime(2077, 1, 1, 11, 10, 0, 0))
                { Services = new[] { "TSERV" } },
            };

            ContactItem[] contactDetails = [new ContactItem { Type = "email", Value = "test@tempuri.org" }];

            var initialBooking = new Booking { Site = "TEST", Service = "TSERV", From = new DateTime(2077, 1, 1, 10, 0, 0, 0), Duration = 10, ContactDetails = null, Provisional = true };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());
            _availabilityCalculator.Setup(x => x.CalculateAvailability("TEST", "TSERV", expectedFrom, expectedUntil)).ReturnsAsync(availability);
            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");
            _bookingsDocumentStore.Setup(x => x.ConfirmProvisional(It.IsAny<string>(), It.IsAny<IEnumerable<ContactItem>>(), It.IsAny<string>())).ReturnsAsync(BookingConfirmationResult.Success);
            _bookingsDocumentStore.Setup(x => x.GetByReferenceOrDefaultAsync("TEST1")).ReturnsAsync(new Booking { Reference = "TEST1", Site = "TEST", Service = "TSERV", From = new DateTime(2077, 1, 1, 10, 0, 0, 0), Duration = 10, ContactDetails = contactDetails, Provisional = false, AttendeeDetails = new AttendeeDetails() });
            _bookingsDocumentStore.Setup(x => x.GetByReferenceOrDefaultAsync("TEST2")).ReturnsAsync(new Booking { Reference = "TEST2", Site = "TEST", Service = "TSERV", From = new DateTime(2077, 1, 1, 11, 0, 0, 0), Duration = 10, ContactDetails = contactDetails, Provisional = false, AttendeeDetails = new AttendeeDetails() });
            var initialBookingResult = await _bookingsService.MakeBooking(initialBooking);
            await _bookingsService.ConfirmProvisionalBooking(initialBookingResult.Reference, contactDetails, "");

            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST2");

            var rescheduledBooking = new Booking { Site = "TEST", Service = "TSERV", From = new DateTime(2077, 1, 1, 11, 0, 0, 0), Duration = 10, ContactDetails = null, Provisional = true };
            var rescheduledBookingResult = await _bookingsService.MakeBooking(rescheduledBooking);

            var rescheduleResult = await _bookingsService.ConfirmProvisionalBooking(rescheduledBooking.Reference, contactDetails, initialBookingResult.Reference);

            _messageBus.Verify(x => x.Send(It.Is<BookingRescheduled>(e => e.Reference == rescheduledBookingResult.Reference)), Times.Once);
        }

        [Fact]
        public async Task MakeBooking_FlagsBookingForReminder_WhenBooking()
        {
            var expectedFrom = new DateOnly(2077, 1, 1);
            var expectedUntil = expectedFrom.AddDays(1);
            var availability = new[] { new SessionInstance(new DateTime(2077, 1, 1, 10, 0, 0, 0), new DateTime(2077, 1, 1, 10, 10, 0, 0)) { Services = new[] { "TSERV" } } };

            var booking = new Booking { Site = "TEST", Service = "TSERV", From = new DateTime(2077, 1, 1, 10, 0, 0, 0), Duration = 10, ContactDetails = [new ContactItem()], AttendeeDetails = new AttendeeDetails() };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());
            _availabilityCalculator.Setup(x => x.CalculateAvailability("TEST", "TSERV", expectedFrom, expectedUntil)).ReturnsAsync(availability);
            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");

            booking.ReminderSent = true; // make sure we're not just testing the default value of False

            var result = await _bookingsService.MakeBooking(booking);

            Assert.False(booking.ReminderSent);
        }

        [Fact]
        public async Task MakeBooking_ReturnsSuccess_WhenSlotIsAvailable()
        {
            var expectedFrom = new DateOnly(2077, 1, 1);
            var expectedUntil = expectedFrom.AddDays(1);
            var availability = new[] { new SessionInstance(new DateTime(2077, 1, 1, 10, 0, 0, 0), new DateTime(2077, 1, 1, 10, 10, 0, 0)) { Services = new[] { "TSERV" } } };

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

        [Fact]
        public async Task CancelBooking_ReturnsNonSuccess_WhenInvalidReference()
        {
            _bookingsDocumentStore.Setup(x => x.GetByReferenceOrDefaultAsync(It.IsAny<string>())).Returns(Task.FromResult((Booking)null));
            var result = await _bookingsService.CancelBooking("some-reference");
            Assert.Equal(BookingCancellationResult.NotFound, result);
        }

        [Fact]
        public async Task CancelBooking_CancelsBookingInDatabase()
        {
            var site = "some-site";
            var bookingRef = "some-booking";

            var updateMock = new Mock<IDocumentUpdate<Booking>>();
            updateMock.Setup(x => x.UpdateProperty(b => b.Outcome, "Cancelled")).Returns(updateMock.Object).Verifiable();

            _bookingsDocumentStore.Setup(x => x.GetByReferenceOrDefaultAsync(It.IsAny<string>())).Returns(Task.FromResult(new Booking() { Site = site }));
            _bookingsDocumentStore.Setup(x => x.BeginUpdate(site, bookingRef)).Returns(updateMock.Object).Verifiable();

            await _bookingsService.CancelBooking(bookingRef);

            _bookingsDocumentStore.VerifyAll();
            updateMock.VerifyAll();
        }

        [Fact]
        public async Task CancelBooking_RaisesNotificationEvent()
        {
            var site = "some-site";
            var bookingRef = "some-booking";

            var updateMock = new Mock<IDocumentUpdate<Booking>>();
            updateMock.Setup(x => x.UpdateProperty(b => b.Outcome, "Cancelled")).Returns(updateMock.Object);

            _bookingsDocumentStore.Setup(x => x.GetByReferenceOrDefaultAsync(It.IsAny<string>())).Returns(Task.FromResult(new Booking { Reference = bookingRef, Site = site}));
            _bookingsDocumentStore.Setup(x => x.BeginUpdate(site, bookingRef)).Returns(updateMock.Object);

            _messageBus.Setup(x => x.Send(It.Is<BookingCancelled>(e => e.Site == site && e.Reference == bookingRef))).Verifiable();

            await _bookingsService.CancelBooking(bookingRef);

            _messageBus.VerifyAll();
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
