using Nhs.Appointments.Core.Concurrency;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;

namespace Nhs.Appointments.Core.UnitTests
{
    public class BookingWriteServiceTests
    {
        private const string MockSite = "some-site";
        private readonly Mock<IAvailabilityCreatedEventStore> _availabilityCreatedEventStore = new();
        private readonly Mock<IAvailabilityStore> _availabilityStore = new();

        private readonly Mock<IBookingAvailabilityStateService> _bookingAvailabilityStateService = new();

        private readonly Mock<IBookingQueryService> _bookingQueryService = new();
        private readonly Mock<IBookingsDocumentStore> _bookingsDocumentStore = new();
        private readonly Mock<IMessageBus> _messageBus = new();
        private readonly Mock<IReferenceNumberProvider> _referenceNumberProvider = new();
        private readonly Mock<ISiteLeaseManager> _siteLeaseManager = new();
        private BookingWriteService _sut;

        public BookingWriteServiceTests()
        {
            _sut = new BookingWriteService(
                _bookingsDocumentStore.Object,
                _bookingQueryService.Object,
                _referenceNumberProvider.Object,
                _siteLeaseManager.Object,
                _bookingAvailabilityStateService.Object,
                new EventFactory(),
                _messageBus.Object,
                TimeProvider.System);
        }

        [Fact]
        public async Task MakeBooking_AcquiresLock_WhenBooking()
        {
            var expectedFrom = new DateOnly(2077, 1, 1);
            var expectedUntil = expectedFrom.AddDays(1);
            SessionInstance[] availability =
            [
                new(new DateTime(2077, 1, 1, 9, 0, 0, 0), new DateTime(2077, 1, 1, 12, 0, 0, 0))
            ];

            var booking = new Booking
            {
                Site = "TEST", Service = "TSERV", From = new DateTime(2077, 1, 1, 10, 0, 0, 0), Duration = 10
            };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());
            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");

            var bookingQueryService = new BookingQueryService(_bookingsDocumentStore.Object, TimeProvider.System);
            var availabilityQueryService =
                new AvailabilityQueryService(_availabilityStore.Object, _availabilityCreatedEventStore.Object);

            var leaseManager = new FakeLeaseManager();
            var bookingService = new BookingWriteService(_bookingsDocumentStore.Object, bookingQueryService,
                _referenceNumberProvider.Object,
                leaseManager, new BookingAvailabilityStateService(availabilityQueryService, bookingQueryService),
                new EventFactory(), _messageBus.Object, TimeProvider.System);

            var task = Task.Run(() => bookingService.MakeBooking(booking));
            await Task.Delay(100);
            task.IsCompleted.Should().BeFalse();

            leaseManager.WaitHandle.Set();

            await Task.Delay(1000);
            task.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task MakeBooking_RaisesNotificationEvent_WhenBooking()
        {
            var availability = new[]
            {
                new SessionInstance(new DateTime(2077, 1, 1, 10, 0, 0, 0), new DateTime(2077, 1, 1, 10, 10, 0, 0))
                {
                    Services = ["TSERV"]
                }
            };

            var booking = new Booking
            {
                Site = MockSite,
                Service = "TSERV",
                From = new DateTime(2077, 1, 1, 10, 0, 0, 0),
                Duration = 10,
                ContactDetails = [new ContactItem { Type = ContactItemType.Email, Value = "test@test.com" }],
                AttendeeDetails = new AttendeeDetails(),
                Status = AppointmentStatus.Booked
            };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());

            MockAvailability(availability);

            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");

            var result = await _sut.MakeBooking(booking);

            _messageBus.Verify(x => x.Send(It.Is<BookingMade>(e => e.Reference == booking.Reference)));
        }

        private void MockAvailability(SessionInstance[] availability)
        {
            _bookingAvailabilityStateService.Setup(x => x.GetAvailableSlots(MockSite,
                    new DateTime(2077, 1, 1, 10, 0, 0, 0),
                    new DateTime(2077, 1, 1, 10, 10, 0, 0)))
                .ReturnsAsync(availability.ToList());
        }

        [Fact]
        public async Task MakeBooking_DoesNotRaiseNotificationEvent_WhenProvisional()
        {
            var availability = new[]
            {
                new SessionInstance(new DateTime(2077, 1, 1, 10, 0, 0, 0), new DateTime(2077, 1, 1, 10, 10, 0, 0))
                {
                    Services = ["TSERV"]
                }
            };

            var booking = new Booking
            {
                Site = MockSite,
                Service = "TSERV",
                From = new DateTime(2077, 1, 1, 10, 0, 0, 0),
                Duration = 10,
                ContactDetails = null,
                Status = AppointmentStatus.Provisional
            };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());

            MockAvailability(availability);

            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");

            var result = await _sut.MakeBooking(booking);

            _messageBus.Verify(x => x.Send(It.Is<BookingMade>(e => e.Reference == booking.Reference)), Times.Never);
        }

        [Fact]
        public async Task RescheduleBooking_IsSuccessful()
        {
            var availability = new[]
            {
                new SessionInstance(new DateTime(2077, 1, 1, 10, 0, 0, 0), new DateTime(2077, 1, 1, 10, 10, 0, 0))
                {
                    Services = ["TSERV"]
                },
                new SessionInstance(new DateTime(2077, 1, 1, 11, 0, 0, 0), new DateTime(2077, 1, 1, 11, 10, 0, 0))
                {
                    Services = ["TSERV"]
                },
            };

            ContactItem[] contactDetails =
                [new ContactItem { Type = ContactItemType.Email, Value = "test@tempuri.org" }];

            var initialBooking = new Booking
            {
                Site = MockSite,
                Service = "TSERV",
                From = new DateTime(2077, 1, 1, 10, 0, 0, 0),
                Duration = 10,
                ContactDetails = null,
                Status = AppointmentStatus.Provisional
            };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());

            _bookingAvailabilityStateService.Setup(x => x.GetAvailableSlots(MockSite,
                    new DateTime(2077, 1, 1, 10, 0, 0, 0),
                    new DateTime(2077, 1, 1, 10, 10, 0, 0)))
                .ReturnsAsync([
                    new SessionInstance(new DateTime(2077, 1, 1, 10, 0, 0, 0),
                        new DateTime(2077, 1, 1, 10, 10, 0, 0)) { Services = ["TSERV"] }
                ]);

            _bookingAvailabilityStateService.Setup(x => x.GetAvailableSlots(MockSite,
                    new DateTime(2077, 1, 1, 11, 0, 0, 0),
                    new DateTime(2077, 1, 1, 11, 10, 0, 0)))
                .ReturnsAsync([
                    new SessionInstance(new DateTime(2077, 1, 1, 11, 0, 0, 0),
                        new DateTime(2077, 1, 1, 11, 10, 0, 0)) { Services = ["TSERV"] }
                ]);

            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");
            _bookingsDocumentStore
                .Setup(x => x.ConfirmProvisional(It.IsAny<string>(), It.IsAny<IEnumerable<ContactItem>>(),
                    It.IsAny<string>(), It.IsAny<CancellationReason>()))
                .ReturnsAsync(BookingConfirmationResult.Success);
            _bookingsDocumentStore.Setup(x => x.GetByReferenceOrDefaultAsync("TEST1")).ReturnsAsync(new Booking
            {
                Reference = "TEST1",
                Site = MockSite,
                Service = "TSERV",
                From = new DateTime(2077, 1, 1, 10, 0, 0, 0),
                Duration = 10,
                ContactDetails = contactDetails,
                Status = AppointmentStatus.Booked,
                AttendeeDetails = new AttendeeDetails()
            });
            _bookingsDocumentStore.Setup(x => x.GetByReferenceOrDefaultAsync("TEST2")).ReturnsAsync(new Booking
            {
                Reference = "TEST2",
                Site = MockSite,
                Service = "TSERV",
                From = new DateTime(2077, 1, 1, 11, 0, 0, 0),
                Duration = 10,
                ContactDetails = contactDetails,
                Status = AppointmentStatus.Booked,
                AttendeeDetails = new AttendeeDetails()
            });

            var initialBookingResult = await _sut.MakeBooking(initialBooking);
            await _sut.ConfirmProvisionalBooking(initialBookingResult.Reference, contactDetails, "");

            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST2");

            var rescheduledBooking = new Booking
            {
                Site = MockSite,
                Service = "TSERV",
                From = new DateTime(2077, 1, 1, 11, 0, 0, 0),
                Duration = 10,
                ContactDetails = null,
                Status = AppointmentStatus.Provisional
            };
            var rescheduledBookingResult = await _sut.MakeBooking(rescheduledBooking);

            var rescheduleResult = await _sut.ConfirmProvisionalBooking(rescheduledBooking.Reference,
                contactDetails, initialBookingResult.Reference);

            rescheduleResult.Should().Be(BookingConfirmationResult.Success);

            _bookingsDocumentStore.Verify(x =>
                    x.ConfirmProvisional(rescheduledBooking.Reference, contactDetails, initialBookingResult.Reference,
                        CancellationReason.RescheduledByCitizen),
                Times.Once);
        }

        [Fact]
        public async Task RescheduleBooking_RaisesNotificationEvent()
        {
            ContactItem[] contactDetails =
                [new ContactItem { Type = ContactItemType.Email, Value = "test@tempuri.org" }];

            var initialBooking = new Booking
            {
                Site = MockSite,
                Service = "TSERV",
                From = new DateTime(2077, 1, 1, 10, 0, 0, 0),
                Duration = 10,
                ContactDetails = null,
                Status = AppointmentStatus.Provisional
            };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());

            _bookingAvailabilityStateService.Setup(x => x.GetAvailableSlots(MockSite,
                    new DateTime(2077, 1, 1, 10, 0, 0, 0),
                    new DateTime(2077, 1, 1, 10, 10, 0, 0)))
                .ReturnsAsync(
                [
                    new SessionInstance(new DateTime(2077, 1, 1, 10, 0, 0, 0),
                        new DateTime(2077, 1, 1, 10, 10, 0, 0)) { Services = ["TSERV"] }
                ]);

            _bookingAvailabilityStateService.Setup(x => x.GetAvailableSlots(MockSite,
                    new DateTime(2077, 1, 1, 11, 0, 0, 0),
                    new DateTime(2077, 1, 1, 11, 10, 0, 0)))
                .ReturnsAsync(
                [
                    new SessionInstance(new DateTime(2077, 1, 1, 11, 0, 0, 0),
                        new DateTime(2077, 1, 1, 11, 10, 0, 0)) { Services = ["TSERV"] }
                ]);

            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");
            _bookingsDocumentStore
                .Setup(x => x.ConfirmProvisional(It.IsAny<string>(), It.IsAny<IEnumerable<ContactItem>>(),
                    It.IsAny<string>(), It.IsAny<CancellationReason>()))
                .ReturnsAsync(BookingConfirmationResult.Success);
            _bookingsDocumentStore.Setup(x => x.GetByReferenceOrDefaultAsync("TEST1")).ReturnsAsync(new Booking
            {
                Reference = "TEST1",
                Site = MockSite,
                Service = "TSERV",
                From = new DateTime(2077, 1, 1, 10, 0, 0, 0),
                Duration = 10,
                ContactDetails = contactDetails,
                Status = AppointmentStatus.Booked,
                AttendeeDetails = new AttendeeDetails()
            });
            _bookingsDocumentStore.Setup(x => x.GetByReferenceOrDefaultAsync("TEST2")).ReturnsAsync(new Booking
            {
                Reference = "TEST2",
                Site = MockSite,
                Service = "TSERV",
                From = new DateTime(2077, 1, 1, 11, 0, 0, 0),
                Duration = 10,
                ContactDetails = contactDetails,
                Status = AppointmentStatus.Booked,
                AttendeeDetails = new AttendeeDetails()
            });
            var initialBookingResult = await _sut.MakeBooking(initialBooking);
            await _sut.ConfirmProvisionalBooking(initialBookingResult.Reference, contactDetails, "");

            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST2");

            var rescheduledBooking = new Booking
            {
                Site = MockSite,
                Service = "TSERV",
                From = new DateTime(2077, 1, 1, 11, 0, 0, 0),
                Duration = 10,
                ContactDetails = null,
                Status = AppointmentStatus.Provisional
            };
            var rescheduledBookingResult = await _sut.MakeBooking(rescheduledBooking);

            var rescheduleResult = await _sut.ConfirmProvisionalBooking(rescheduledBooking.Reference,
                contactDetails, initialBookingResult.Reference);

            _messageBus.Verify(
                x => x.Send(It.Is<BookingRescheduled>(e => e.Reference == rescheduledBookingResult.Reference)),
                Times.Once);

            _bookingsDocumentStore.Verify(x =>
                    x.ConfirmProvisional(rescheduledBooking.Reference, contactDetails, initialBookingResult.Reference,
                        CancellationReason.RescheduledByCitizen),
                Times.Once);
        }

        [Fact]
        public async Task MakeBooking_ReturnsSuccess_WhenSlotIsAvailable()
        {
            var availability = new[]
            {
                new SessionInstance(new DateTime(2077, 1, 1, 10, 0, 0, 0), new DateTime(2077, 1, 1, 10, 10, 0, 0))
                {
                    Services = ["TSERV"]
                }
            };

            var booking = new Booking
            {
                Site = MockSite,
                Service = "TSERV",
                From = new DateTime(2077, 1, 1, 10, 0, 0, 0),
                Duration = 10,
                ContactDetails = [new ContactItem()],
                AttendeeDetails = new AttendeeDetails(),
                Status = AppointmentStatus.Booked
            };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());

            MockAvailability(availability);

            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");

            var result = await _sut.MakeBooking(booking);
            result.Success.Should().BeTrue();
            result.Reference.Should().Be("TEST1");
        }

        [Fact]
        public async Task MakeBooking_FlagsBookingForReminder_WhenBooking()
        {
            var availability = new[]
            {
                new SessionInstance(new DateTime(2077, 1, 1, 10, 0, 0, 0), new DateTime(2077, 1, 1, 10, 10, 0, 0))
                {
                    Services = ["TSERV"]
                }
            };

            var booking = new Booking
            {
                Site = MockSite,
                Service = "TSERV",
                From = new DateTime(2077, 1, 1, 10, 0, 0, 0),
                Duration = 10,
                ContactDetails = [new ContactItem()],
                AttendeeDetails = new AttendeeDetails(),
                Status = AppointmentStatus.Booked
            };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());

            MockAvailability(availability);

            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");

            booking.ReminderSent = true; // make sure we're not just testing the default value of False

            var result = await _sut.MakeBooking(booking);

            Assert.False(booking.ReminderSent);
        }

        [Fact]
        public async Task MakeBooking_ReturnsNonSuccess_WhenSlotIsNotAvailable()
        {
            var booking = new Booking
            {
                Site = MockSite,
                Service = "TSERV",
                From = new DateTime(2077, 1, 1, 13, 0, 0, 0),
                Duration = 10,
                Status = AppointmentStatus.Booked
            };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());

            _bookingAvailabilityStateService.Setup(x => x.GetAvailableSlots(MockSite,
                    new DateTime(2077, 1, 1, 13, 0, 0, 0),
                    new DateTime(2077, 1, 1, 13, 10, 0, 0)))
                .ReturnsAsync(
                [
                    new SessionInstance(new DateTime(2077, 1, 1, 9, 0, 0, 0), new DateTime(2077, 1, 1, 12, 0, 0, 0))
                    {
                        Services = ["TSERV"]
                    },
                    //test a different session that could have been used, had it been the right service for the booking
                    new SessionInstance(new DateTime(2077, 1, 1, 13, 0, 0, 0),
                        new DateTime(2077, 1, 1, 13, 10, 0, 0)) { Services = ["TDIFFSERV"] }
                ]);

            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");

            var result = await _sut.MakeBooking(booking);
            result.Success.Should().BeFalse();
            result.Reference.Should().Be(string.Empty);
        }

        [Fact]
        public async Task CancelBooking_ReturnsNonSuccess_WhenInvalidReference()
        {
            _bookingsDocumentStore.Setup(x => x.GetByReferenceOrDefaultAsync(It.IsAny<string>()))
                .Returns(Task.FromResult<Booking>(null));
            var result = await _sut.CancelBooking("some-reference", "TEST01", CancellationReason.CancelledByCitizen);
            Assert.Equal(BookingCancellationResult.NotFound, result);
        }

        [Fact]
        public async Task CancelBooking_CancelsBookingInDatabase()
        {
            var site = "some-site";
            var bookingRef = "some-booking";

            _bookingsDocumentStore.Setup(x => x.GetByReferenceOrDefaultAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new Booking { Site = site, ContactDetails = [] }));
            _bookingsDocumentStore
                .Setup(x => x.UpdateStatus(bookingRef, AppointmentStatus.Cancelled, AvailabilityStatus.Unknown,
                    CancellationReason.CancelledByCitizen))
                .ReturnsAsync(true).Verifiable();

            await _sut.CancelBooking(bookingRef, site, CancellationReason.CancelledByCitizen);

            _bookingsDocumentStore.VerifyAll();
        }

        [Fact]
        public async Task CancelBooking_RaisesNotificationEvent()
        {
            var site = "some-site";
            var bookingRef = "some-booking";

            var updateMock = new Mock<IDocumentUpdate<Booking>>();
            updateMock.Setup(x => x.UpdateProperty(b => b.Status, AppointmentStatus.Cancelled))
                .Returns(updateMock.Object);

            _bookingsDocumentStore.Setup(x => x.GetByReferenceOrDefaultAsync(It.IsAny<string>())).Returns(
                Task.FromResult(new Booking
                {
                    Reference = bookingRef,
                    Site = site,
                    ContactDetails = [new ContactItem { Type = ContactItemType.Email, Value = "test@tempuri.org" }]
                }));
            _bookingsDocumentStore.Setup(x => x.BeginUpdate(site, bookingRef)).Returns(updateMock.Object);

            _messageBus.Setup(x => x.Send(It.Is<BookingCancelled[]>(e =>
                e[0].Site == site && e[0].Reference == bookingRef && e[0].NotificationType == NotificationType.Email &&
                e[0].Destination == "test@tempuri.org"))).Verifiable(Times.Once);

            await _sut.CancelBooking(bookingRef, site, CancellationReason.CancelledByCitizen);

            _messageBus.VerifyAll();
        }

        [Fact]
        public async Task CancelBooking_ReturnsNotFoundWhenSiteDoesNotMatch()
        {
            var site = "some-site";
            var bookingRef = "some-booking";

            _bookingsDocumentStore.Setup(x => x.GetByReferenceOrDefaultAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(new Booking { Site = site, ContactDetails = [] }));
            _bookingsDocumentStore
                .Setup(x => x.UpdateStatus(bookingRef, AppointmentStatus.Cancelled, AvailabilityStatus.Unknown, null))
                .ReturnsAsync(true).Verifiable();

            var result = await _sut.CancelBooking(bookingRef, "some-other-site", CancellationReason.CancelledByCitizen);

            result.Should().Be(BookingCancellationResult.NotFound);
        }

        [Theory]
        [InlineData(CancellationReason.CancelledByCitizen, CancellationReason.CancelledByCitizen)]
        [InlineData(CancellationReason.CancelledBySite, CancellationReason.CancelledBySite)]
        public async Task CancelBooking_ValidCancellationReasonIsUsed(CancellationReason reason,
            CancellationReason expectedReason)
        {
            var reference = "BOOK-123";
            var site = "SITE01";
            var booking = new Booking
            {
                Reference = reference, Site = site, From = DateTime.UtcNow, Status = AppointmentStatus.Booked
            };

            _bookingsDocumentStore.Setup(x => x.GetByReferenceOrDefaultAsync(reference)).ReturnsAsync(booking);
            _bookingsDocumentStore.Setup(x =>
                    x.UpdateStatus(reference, AppointmentStatus.Cancelled, AvailabilityStatus.Unknown, expectedReason))
                .ReturnsAsync(true)
                .Verifiable();

            var result = await _sut.CancelBooking(reference, site, reason);

            result.Should().Be(BookingCancellationResult.Success);
            _bookingsDocumentStore.Verify();
        }

        [Fact]
        public async Task ConfirmProvisional_CancellationReasonIsUsed_WhenReschedulingAppointment()
        {
            var contactDetails =
                new List<ContactItem> { new() { Type = ContactItemType.Email, Value = "test.email@domain.com" } };

            _bookingsDocumentStore.Setup(x => x.ConfirmProvisional(It.IsAny<string>(),
                    It.IsAny<IEnumerable<ContactItem>>(), It.IsAny<string>(), It.IsAny<CancellationReason>()))
                .ReturnsAsync(BookingConfirmationResult.Success);
            _bookingsDocumentStore.Setup(x => x.GetByReferenceOrDefaultAsync("test-booking-ref")).ReturnsAsync(
                new Booking
                {
                    Reference = "test-booking-ref",
                    Site = MockSite,
                    Service = "TSERV",
                    From = new DateTime(2077, 1, 1, 10, 0, 0, 0),
                    Duration = 10,
                    ContactDetails = contactDetails.ToArray(),
                    Status = AppointmentStatus.Booked,
                    AttendeeDetails = new AttendeeDetails()
                });

            var result =
                await _sut.ConfirmProvisionalBooking("test-booking-ref", contactDetails, "booking-to-reschedule");

            result.Should().Be(BookingConfirmationResult.Success);
            _bookingsDocumentStore.Verify(x =>
                    x.ConfirmProvisional("test-booking-ref", It.IsAny<IEnumerable<ContactItem>>(),
                        "booking-to-reschedule", CancellationReason.RescheduledByCitizen),
                Times.Once);
        }

        [Fact]
        public async Task RecalculateAppointmentStatuses_SchedulesOrphanedAppointmentsIfPossible()
        {
            var bookings = new List<Booking>
            {
                new()
                {
                    From = new DateTime(2025, 01, 01, 9, 0, 0),
                    Reference = "1",
                    AttendeeDetails = new AttendeeDetails { FirstName = "Daniel", LastName = "Dixon" },
                    Status = AppointmentStatus.Booked,
                    AvailabilityStatus = AvailabilityStatus.Orphaned,
                    Duration = 10,
                    Service = "Service 1"
                },
                new()
                {
                    From = new DateTime(2025, 01, 01, 9, 10, 0),
                    Reference = "2",
                    AttendeeDetails = new AttendeeDetails { FirstName = "Alexander", LastName = "Cooper" },
                    Status = AppointmentStatus.Booked,
                    AvailabilityStatus = AvailabilityStatus.Orphaned,
                    Duration = 10,
                    Service = "Service 1"
                }
            };

            _bookingAvailabilityStateService
                .Setup(x => x.BuildRecalculations(MockSite, It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(
                    new List<BookingAvailabilityUpdate>
                    {
                        new(bookings.First(), AvailabilityUpdateAction.SetToSupported),
                        new(bookings.Last(), AvailabilityUpdateAction.SetToSupported),
                    });

            await _sut.RecalculateAppointmentStatuses(MockSite, new DateOnly(2025, 1, 1));

            _bookingsDocumentStore.Verify(x => x.UpdateAvailabilityStatus(
                    It.Is<string>(s => s == "1"),
                    It.Is<AvailabilityStatus>(s => s == AvailabilityStatus.Supported)),
                Times.Once);

            _bookingsDocumentStore.Verify(x => x.UpdateAvailabilityStatus(
                    It.Is<string>(s => s == "2"),
                    It.Is<AvailabilityStatus>(s => s == AvailabilityStatus.Supported)),
                Times.Once);
        }

        [Fact]
        public async Task RecalculateAppointmentStatuses_OrphansLiveAppointmentsIfTheyCannotBeFulfilled()
        {
            var bookings = new List<Booking>
            {
                new()
                {
                    From = new DateTime(2025, 01, 01, 9, 0, 0),
                    Reference = "1",
                    AttendeeDetails = new AttendeeDetails { FirstName = "Daniel", LastName = "Dixon" },
                    Status = AppointmentStatus.Booked,
                    AvailabilityStatus = AvailabilityStatus.Supported,
                    Duration = 10
                },
                new()
                {
                    From = new DateTime(2025, 01, 01, 9, 10, 0),
                    Reference = "2",
                    AttendeeDetails = new AttendeeDetails { FirstName = "Alexander", LastName = "Cooper" },
                    Status = AppointmentStatus.Booked,
                    AvailabilityStatus = AvailabilityStatus.Supported,
                    Duration = 10
                }
            };

            _bookingAvailabilityStateService
                .Setup(x => x.BuildRecalculations(MockSite, It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(
                    new List<BookingAvailabilityUpdate>
                    {
                        new(bookings.First(), AvailabilityUpdateAction.SetToOrphaned),
                        new(bookings.Last(), AvailabilityUpdateAction.SetToOrphaned),
                    });

            await _sut.RecalculateAppointmentStatuses(MockSite, new DateOnly(2025, 1, 1));

            _bookingsDocumentStore.Verify(x => x.UpdateAvailabilityStatus(
                    It.Is<string>(s => s == "1"),
                    It.Is<AvailabilityStatus>(s => s == AvailabilityStatus.Orphaned)),
                Times.Once);

            _bookingsDocumentStore.Verify(x => x.UpdateAvailabilityStatus(
                    It.Is<string>(s => s == "2"),
                    It.Is<AvailabilityStatus>(s => s == AvailabilityStatus.Orphaned)),
                Times.Once);
        }

        [Fact]
        public async Task RecalculateAppointmentStatuses_DeletesProvisionalAppointments()
        {
            const string service = "Service 1";

            IEnumerable<Booking> bookings = new List<Booking>
            {
                new()
                {
                    From = new DateTime(2025, 01, 01, 9, 0, 0),
                    Reference = "1",
                    AttendeeDetails = new AttendeeDetails { FirstName = "Daniel", LastName = "Dixon" },
                    Status = AppointmentStatus.Booked,
                    AvailabilityStatus = AvailabilityStatus.Supported,
                    Duration = 10,
                    Site = MockSite,
                    Service = service
                },
                new()
                {
                    From = new DateTime(2025, 01, 01, 9, 10, 0),
                    Reference = "2",
                    AttendeeDetails = new AttendeeDetails { FirstName = "Alexander", LastName = "Cooper" },
                    Status = AppointmentStatus.Provisional,
                    Duration = 10,
                    Site = MockSite,
                    AvailabilityStatus = AvailabilityStatus.Supported,
                    Service = service
                }
            };

            _bookingAvailabilityStateService
                .Setup(x => x.BuildRecalculations(MockSite, It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(
                    new List<BookingAvailabilityUpdate>
                    {
                        new(bookings.First(), AvailabilityUpdateAction.SetToOrphaned),
                        new(bookings.Last(), AvailabilityUpdateAction.ProvisionalToDelete),
                    });

            await _sut.RecalculateAppointmentStatuses(MockSite, new DateOnly(2025, 1, 1));

            _bookingsDocumentStore.Verify(x => x.UpdateAvailabilityStatus(
                    It.Is<string>(s => s == "1"),
                    It.Is<AvailabilityStatus>(s => s == AvailabilityStatus.Orphaned)),
                Times.Once);

            _bookingsDocumentStore.Verify(x => x.DeleteBooking(
                    It.Is<string>(s => s == "2"),
                    It.Is<string>(s => s == MockSite)),
                Times.Once);
        }

        [Fact]
        public async Task RecalculateAppointmentStatuses_MakesNoChangesIfAllAppointmentsAreStillValid()
        {
            _bookingAvailabilityStateService
                .Setup(x => x.BuildRecalculations(MockSite, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<BookingAvailabilityUpdate>());

            await _sut.RecalculateAppointmentStatuses(MockSite, new DateOnly(2025, 1, 1));

            _bookingsDocumentStore.Verify(
                x => x.UpdateStatus(It.IsAny<string>(), It.IsAny<AppointmentStatus>(),
                    It.IsAny<AvailabilityStatus>(), It.IsAny<CancellationReason>()),
                Times.Never);
        }

        [Fact]
        public async Task MakeBooking_CallsAllocationStateService_WhenBooking()
        {
            var booking = new Booking { Site = "TEST", Service = "TSERV", From = new DateTime(2077, 1, 1) };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());
            _bookingAvailabilityStateService
                .Setup(x => x.GetAvailableSlots(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<SessionInstance>());
            await _sut.MakeBooking(booking);
            _bookingAvailabilityStateService.Verify(x =>
                x.GetAvailableSlots(booking.Site, booking.From, booking.From.AddMinutes(booking.Duration)));
        }

        [Fact]
        public async Task RecalculateAppointmentStatuses_DoesntGoDownSingleServiceCodePath()
        {
            _bookingAvailabilityStateService
                .Setup(x => x.BuildRecalculations(MockSite, It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(
                    new List<BookingAvailabilityUpdate>());

            await _sut.RecalculateAppointmentStatuses(MockSite, new DateOnly(2025, 1, 1));

            //multiService code path
            _bookingAvailabilityStateService.Verify(x => x.BuildRecalculations(
                    MockSite, It.IsAny<DateTime>(), It.IsAny<DateTime>()),
                Times.Once);

            //singleService code path
            _bookingQueryService.Verify(x => x.GetBookings(
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(), MockSite),
                Times.Never);
            _availabilityStore.Verify(x => x.GetSessions(
                    MockSite, It.IsAny<DateOnly>(), It.IsAny<DateOnly>()),
                Times.Never);
        }

        [Fact]
        public async Task CancelAllBookingsInDayAsync_CallsBookingStore()
        {
            var date = new DateOnly(2025, 1, 1);
            _bookingsDocumentStore.Setup(x => x.CancelAllBookingsInDay(It.IsAny<string>(), It.IsAny<DateOnly>()))
                .ReturnsAsync((5, 1));

            var result = await _sut.CancelAllBookingsInDayAsync("TEST_SITE_123", date);

            result.Should().Be((5, 1));

            _bookingsDocumentStore.Verify(x => x.CancelAllBookingsInDay("TEST_SITE_123", date), Times.Once);
        }
    }

    public class FakeLeaseManager : ISiteLeaseManager
    {
        public FakeLeaseManager()
        {
            WaitHandle = new AutoResetEvent(false);
        }


        public AutoResetEvent WaitHandle { get; }

        public ISiteLeaseContext Acquire(string site)
        {
            WaitHandle.WaitOne();
            return new FakeLeaseContext();
        }
    }

    public class FakeLeaseContext : ISiteLeaseContext
    {
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
