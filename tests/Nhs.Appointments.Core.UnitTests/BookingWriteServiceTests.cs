using Nhs.Appointments.Core.Concurrency;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;

#pragma warning disable CS0618 // Keep availabilityCalculator around until MultipleServicesEnabled is stable

namespace Nhs.Appointments.Core.UnitTests
{
    public abstract class BookingWriteBaseServiceTests : FeatureToggledTests
    {
        private const string MockSite = "some-site";
        private readonly Mock<IAvailabilityCalculator> _availabilityCalculator = new();
        private readonly Mock<IAvailabilityCreatedEventStore> _availabilityCreatedEventStore = new();
        private readonly Mock<IAvailabilityStore> _availabilityStore = new();
        private readonly Mock<IBookingsDocumentStore> _bookingsDocumentStore = new();
        private readonly BookingWriteService _bookingWriteService;
        private readonly Mock<IMessageBus> _messageBus = new();
        private readonly Mock<IReferenceNumberProvider> _referenceNumberProvider = new();
        private readonly Mock<ISiteLeaseManager> _siteLeaseManager = new();

        protected BookingWriteBaseServiceTests(Type testClassType) : base(testClassType)
        {
            var bookingQueryService = new BookingQueryService(_bookingsDocumentStore.Object, TimeProvider.System);
            var availabilityQueryService =
                new AvailabilityQueryService(_availabilityStore.Object, _availabilityCreatedEventStore.Object);

            _bookingWriteService = new BookingWriteService(
                _bookingsDocumentStore.Object,
                bookingQueryService,
                _referenceNumberProvider.Object,
                _siteLeaseManager.Object,
                _availabilityStore.Object,
                _availabilityCalculator.Object,
                new AllocationStateService(availabilityQueryService, bookingQueryService),
                new EventFactory(),
                _messageBus.Object,
                TimeProvider.System,
                _featureToggleHelper.Object);
        }

        [Fact]
        public async Task MakeBooking_AcquiresLock_WhenBooking()
        {
            var expectedFrom = new DateOnly(2077, 1, 1);
            var expectedUntil = expectedFrom.AddDays(1);
            var availability = new[]
            {
                new SessionInstance(new DateTime(2077, 1, 1, 9, 0, 0, 0), new DateTime(2077, 1, 1, 12, 0, 0, 0))
            };

            var booking = new Booking
            {
                Site = "TEST", Service = "TSERV", From = new DateTime(2077, 1, 1, 10, 0, 0, 0), Duration = 10
            };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());
            _availabilityCalculator.Setup(x => x.CalculateAvailability("TEST", "TSERV", expectedFrom, expectedUntil))
                .ReturnsAsync(availability);
            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");

            var bookingQueryService = new BookingQueryService(_bookingsDocumentStore.Object, TimeProvider.System);
            var availabilityQueryService =
                new AvailabilityQueryService(_availabilityStore.Object, _availabilityCreatedEventStore.Object);

            var leaseManager = new FakeLeaseManager();
            var bookingService = new BookingWriteService(_bookingsDocumentStore.Object, bookingQueryService,
                _referenceNumberProvider.Object,
                leaseManager, _availabilityStore.Object, _availabilityCalculator.Object,
                new AllocationStateService(availabilityQueryService, bookingQueryService),
                new EventFactory(), _messageBus.Object, TimeProvider.System, _featureToggleHelper.Object);

            var task = Task.Run(() => bookingService.MakeBooking(booking));
            await Task.Delay(100);
            task.IsCompleted.Should().BeFalse();

            leaseManager.WaitHandle.Set();

            await Task.Delay(1000);
            task.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task MakeBooking_CallsAvailabilityCalculator_WhenBooking()
        {
            var expectedFrom = new DateOnly(2077, 1, 1);
            var expectedUntil = expectedFrom.AddDays(1);

            var booking = new Booking { Site = "TEST", Service = "TSERV", From = new DateTime(2077, 1, 1) };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());
            var result = await _bookingWriteService.MakeBooking(booking);

            _availabilityCalculator.Verify(x => x.CalculateAvailability("TEST", "TSERV", expectedFrom, expectedUntil));
        }

        [Fact]
        public async Task MakeBooking_RaisesNotificationEvent_WhenBooking()
        {
            var expectedFrom = new DateOnly(2077, 1, 1);
            var expectedUntil = expectedFrom.AddDays(1);
            var availability = new[]
            {
                new SessionInstance(new DateTime(2077, 1, 1, 10, 0, 0, 0), new DateTime(2077, 1, 1, 10, 10, 0, 0))
                {
                    Services = new[] { "TSERV" }
                }
            };

            var booking = new Booking
            {
                Site = "TEST",
                Service = "TSERV",
                From = new DateTime(2077, 1, 1, 10, 0, 0, 0),
                Duration = 10,
                ContactDetails = [new ContactItem { Type = ContactItemType.Email, Value = "test@test.com" }],
                AttendeeDetails = new AttendeeDetails(),
                Status = AppointmentStatus.Booked
            };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());
            _availabilityCalculator.Setup(x => x.CalculateAvailability("TEST", "TSERV", expectedFrom, expectedUntil))
                .ReturnsAsync(availability);
            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");

            var result = await _bookingWriteService.MakeBooking(booking);

            _messageBus.Verify(x => x.Send(It.Is<BookingMade>(e => e.Reference == booking.Reference)));
        }

        [Fact]
        public async Task MakeBooking_DoesNotRaiseNotificationEvent_WhenProvisional()
        {
            var expectedFrom = new DateOnly(2077, 1, 1);
            var expectedUntil = expectedFrom.AddDays(1);
            var availability = new[]
            {
                new SessionInstance(new DateTime(2077, 1, 1, 10, 0, 0, 0), new DateTime(2077, 1, 1, 10, 10, 0, 0))
                {
                    Services = new[] { "TSERV" }
                }
            };

            var booking = new Booking
            {
                Site = "TEST",
                Service = "TSERV",
                From = new DateTime(2077, 1, 1, 10, 0, 0, 0),
                Duration = 10,
                ContactDetails = null,
                Status = AppointmentStatus.Provisional
            };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());
            _availabilityCalculator.Setup(x => x.CalculateAvailability("TEST", "TSERV", expectedFrom, expectedUntil))
                .ReturnsAsync(availability);
            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");

            var result = await _bookingWriteService.MakeBooking(booking);

            _messageBus.Verify(x => x.Send(It.Is<BookingMade>(e => e.Reference == booking.Reference)), Times.Never);
        }

        [Fact]
        public async Task RescheduleBooking_IsSuccessful()
        {
            var expectedFrom = new DateOnly(2077, 1, 1);
            var expectedUntil = expectedFrom.AddDays(1);
            var availability = new[]
            {
                new SessionInstance(new DateTime(2077, 1, 1, 10, 0, 0, 0), new DateTime(2077, 1, 1, 10, 10, 0, 0))
                {
                    Services = new[] { "TSERV" }
                },
                new SessionInstance(new DateTime(2077, 1, 1, 11, 0, 0, 0), new DateTime(2077, 1, 1, 11, 10, 0, 0))
                {
                    Services = new[] { "TSERV" }
                },
            };

            ContactItem[] contactDetails =
                [new ContactItem { Type = ContactItemType.Email, Value = "test@tempuri.org" }];

            var initialBooking = new Booking
            {
                Site = "TEST",
                Service = "TSERV",
                From = new DateTime(2077, 1, 1, 10, 0, 0, 0),
                Duration = 10,
                ContactDetails = null,
                Status = AppointmentStatus.Provisional
            };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());
            _availabilityCalculator.Setup(x => x.CalculateAvailability("TEST", "TSERV", expectedFrom, expectedUntil))
                .ReturnsAsync(availability);
            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");
            _bookingsDocumentStore
                .Setup(x => x.ConfirmProvisional(It.IsAny<string>(), It.IsAny<IEnumerable<ContactItem>>(),
                    It.IsAny<string>())).ReturnsAsync(BookingConfirmationResult.Success);
            _bookingsDocumentStore.Setup(x => x.GetByReferenceOrDefaultAsync("TEST1")).ReturnsAsync(new Booking
            {
                Reference = "TEST1",
                Site = "TEST",
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
                Site = "TEST",
                Service = "TSERV",
                From = new DateTime(2077, 1, 1, 11, 0, 0, 0),
                Duration = 10,
                ContactDetails = contactDetails,
                Status = AppointmentStatus.Booked,
                AttendeeDetails = new AttendeeDetails()
            });
            var initialBookingResult = await _bookingWriteService.MakeBooking(initialBooking);
            await _bookingWriteService.ConfirmProvisionalBooking(initialBookingResult.Reference, contactDetails, "");

            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST2");

            var rescheduledBooking = new Booking
            {
                Site = "TEST",
                Service = "TSERV",
                From = new DateTime(2077, 1, 1, 11, 0, 0, 0),
                Duration = 10,
                ContactDetails = null,
                Status = AppointmentStatus.Provisional
            };
            var rescheduledBookingResult = await _bookingWriteService.MakeBooking(rescheduledBooking);

            var rescheduleResult = await _bookingWriteService.ConfirmProvisionalBooking(rescheduledBooking.Reference,
                contactDetails, initialBookingResult.Reference);

            rescheduleResult.Should().Be(BookingConfirmationResult.Success);
        }

        [Fact]
        public async Task RescheduleBooking_RaisesNotificationEvent()
        {
            var expectedFrom = new DateOnly(2077, 1, 1);
            var expectedUntil = expectedFrom.AddDays(1);
            var availability = new[]
            {
                new SessionInstance(new DateTime(2077, 1, 1, 10, 0, 0, 0), new DateTime(2077, 1, 1, 10, 10, 0, 0))
                {
                    Services = new[] { "TSERV" }
                },
                new SessionInstance(new DateTime(2077, 1, 1, 11, 0, 0, 0), new DateTime(2077, 1, 1, 11, 10, 0, 0))
                {
                    Services = new[] { "TSERV" }
                },
            };

            ContactItem[] contactDetails =
                [new ContactItem { Type = ContactItemType.Email, Value = "test@tempuri.org" }];

            var initialBooking = new Booking
            {
                Site = "TEST",
                Service = "TSERV",
                From = new DateTime(2077, 1, 1, 10, 0, 0, 0),
                Duration = 10,
                ContactDetails = null,
                Status = AppointmentStatus.Provisional
            };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());
            _availabilityCalculator.Setup(x => x.CalculateAvailability("TEST", "TSERV", expectedFrom, expectedUntil))
                .ReturnsAsync(availability);
            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");
            _bookingsDocumentStore
                .Setup(x => x.ConfirmProvisional(It.IsAny<string>(), It.IsAny<IEnumerable<ContactItem>>(),
                    It.IsAny<string>())).ReturnsAsync(BookingConfirmationResult.Success);
            _bookingsDocumentStore.Setup(x => x.GetByReferenceOrDefaultAsync("TEST1")).ReturnsAsync(new Booking
            {
                Reference = "TEST1",
                Site = "TEST",
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
                Site = "TEST",
                Service = "TSERV",
                From = new DateTime(2077, 1, 1, 11, 0, 0, 0),
                Duration = 10,
                ContactDetails = contactDetails,
                Status = AppointmentStatus.Booked,
                AttendeeDetails = new AttendeeDetails()
            });
            var initialBookingResult = await _bookingWriteService.MakeBooking(initialBooking);
            await _bookingWriteService.ConfirmProvisionalBooking(initialBookingResult.Reference, contactDetails, "");

            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST2");

            var rescheduledBooking = new Booking
            {
                Site = "TEST",
                Service = "TSERV",
                From = new DateTime(2077, 1, 1, 11, 0, 0, 0),
                Duration = 10,
                ContactDetails = null,
                Status = AppointmentStatus.Provisional
            };
            var rescheduledBookingResult = await _bookingWriteService.MakeBooking(rescheduledBooking);

            var rescheduleResult = await _bookingWriteService.ConfirmProvisionalBooking(rescheduledBooking.Reference,
                contactDetails, initialBookingResult.Reference);

            _messageBus.Verify(
                x => x.Send(It.Is<BookingRescheduled>(e => e.Reference == rescheduledBookingResult.Reference)),
                Times.Once);
        }

        [Fact]
        public async Task MakeBooking_FlagsBookingForReminder_WhenBooking()
        {
            var expectedFrom = new DateOnly(2077, 1, 1);
            var expectedUntil = expectedFrom.AddDays(1);
            var availability = new[]
            {
                new SessionInstance(new DateTime(2077, 1, 1, 10, 0, 0, 0), new DateTime(2077, 1, 1, 10, 10, 0, 0))
                {
                    Services = new[] { "TSERV" }
                }
            };

            var booking = new Booking
            {
                Site = "TEST",
                Service = "TSERV",
                From = new DateTime(2077, 1, 1, 10, 0, 0, 0),
                Duration = 10,
                ContactDetails = [new ContactItem()],
                AttendeeDetails = new AttendeeDetails(),
                Status = AppointmentStatus.Booked
            };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());
            _availabilityCalculator.Setup(x => x.CalculateAvailability("TEST", "TSERV", expectedFrom, expectedUntil))
                .ReturnsAsync(availability);
            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");

            booking.ReminderSent = true; // make sure we're not just testing the default value of False

            var result = await _bookingWriteService.MakeBooking(booking);

            Assert.False(booking.ReminderSent);
        }

        [Fact]
        public async Task MakeBooking_ReturnsSuccess_WhenSlotIsAvailable()
        {
            var expectedFrom = new DateOnly(2077, 1, 1);
            var expectedUntil = expectedFrom.AddDays(1);
            var availability = new[]
            {
                new SessionInstance(new DateTime(2077, 1, 1, 10, 0, 0, 0), new DateTime(2077, 1, 1, 10, 10, 0, 0))
                {
                    Services = new[] { "TSERV" }
                }
            };

            var booking = new Booking
            {
                Site = "TEST",
                Service = "TSERV",
                From = new DateTime(2077, 1, 1, 10, 0, 0, 0),
                Duration = 10,
                ContactDetails = [new ContactItem()],
                AttendeeDetails = new AttendeeDetails(),
                Status = AppointmentStatus.Booked
            };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());
            _availabilityCalculator.Setup(x => x.CalculateAvailability("TEST", "TSERV", expectedFrom, expectedUntil))
                .ReturnsAsync(availability);
            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");

            var result = await _bookingWriteService.MakeBooking(booking);
            result.Success.Should().BeTrue();
            result.Reference.Should().Be("TEST1");
        }

        [Fact]
        public async Task MakeBooking_ReturnsNonSuccess_WhenSlotIsNotAvailable()
        {
            var expectedFrom = new DateOnly(2077, 1, 1);
            var expectedUntil = expectedFrom.AddDays(1);
            var availability = new[]
            {
                new SessionInstance(new DateTime(2077, 1, 1, 9, 0, 0, 0), new DateTime(2077, 1, 1, 12, 0, 0, 0))
            };

            var booking = new Booking
            {
                Site = "TEST",
                Service = "TSERV",
                From = new DateTime(2077, 1, 1, 13, 0, 0, 0),
                Duration = 10,
                Status = AppointmentStatus.Booked
            };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());
            _availabilityCalculator.Setup(x => x.CalculateAvailability("TEST", "TSERV", expectedFrom, expectedUntil))
                .ReturnsAsync(availability);
            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");

            var result = await _bookingWriteService.MakeBooking(booking);
            result.Success.Should().BeFalse();
            result.Reference.Should().Be(string.Empty);
        }

        [Fact]
        public async Task CancelBooking_ReturnsNonSuccess_WhenInvalidReference()
        {
            _bookingsDocumentStore.Setup(x => x.GetByReferenceOrDefaultAsync(It.IsAny<string>()))
                .Returns(Task.FromResult<Booking>(null));
            var result = await _bookingWriteService.CancelBooking("some-reference", "TEST01");
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
                .Setup(x => x.UpdateStatus(bookingRef, AppointmentStatus.Cancelled, AvailabilityStatus.Unknown))
                .ReturnsAsync(true).Verifiable();

            await _bookingWriteService.CancelBooking(bookingRef, site);

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

            await _bookingWriteService.CancelBooking(bookingRef, site);

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
                .Setup(x => x.UpdateStatus(bookingRef, AppointmentStatus.Cancelled, AvailabilityStatus.Unknown))
                .ReturnsAsync(true).Verifiable();

            var result = await _bookingWriteService.CancelBooking(bookingRef, "some-other-site");

            result.Should().Be(BookingCancellationResult.NotFound);
        }

        [Fact]
        public async Task RecalculateAppointmentStatuses_MakesNoChangesIfAllAppointmentsAreStillValid()
        {
            IEnumerable<Booking> bookings = new List<Booking>
            {
                new()
                {
                    From = new DateTime(2025, 01, 01, 9, 0, 0),
                    Reference = "1",
                    AttendeeDetails = new AttendeeDetails { FirstName = "Daniel", LastName = "Dixon" },
                    Status = AppointmentStatus.Booked,
                    Duration = 10,
                    Service = "Service 1"
                },
                new()
                {
                    From = new DateTime(2025, 01, 01, 9, 10, 0),
                    Reference = "2",
                    AttendeeDetails = new AttendeeDetails { FirstName = "Alexander", LastName = "Cooper" },
                    Status = AppointmentStatus.Booked,
                    Duration = 10,
                    Service = "Service 1"
                },
                new()
                {
                    From = new DateTime(2025, 01, 01, 9, 20, 0),
                    Reference = "3",
                    AttendeeDetails = new AttendeeDetails { FirstName = "Alexander", LastName = "Brown" },
                    Status = AppointmentStatus.Booked,
                    Duration = 10,
                    Service = "Service 1"
                },
                new()
                {
                    From = new DateTime(2025, 01, 01, 9, 30, 0),
                    Reference = "4",
                    AttendeeDetails = new AttendeeDetails { FirstName = "Bob", LastName = "Dawson" },
                    Status = AppointmentStatus.Booked,
                    Duration = 10,
                    Service = "Service 1"
                }
            };

            _bookingsDocumentStore
                .Setup(x => x.GetInDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), MockSite))
                .ReturnsAsync(bookings);

            var sessions = new List<SessionInstance>
            {
                new(new DateTime(2025, 01, 01, 9, 0, 0), new DateTime(2025, 01, 1, 12, 0, 0))
                {
                    Services = ["Service 1"], SlotLength = 10, Capacity = 1
                }
            };

            _availabilityStore
                .Setup(x => x.GetSessions(
                    It.Is<string>(s => s == MockSite),
                    It.IsAny<DateOnly>(),
                    It.IsAny<DateOnly>()))
                .ReturnsAsync(sessions);

            await _bookingWriteService.RecalculateAppointmentStatuses(MockSite, new DateOnly(2025, 1, 1));

            _availabilityStore.Verify(a =>
                a.GetSessions(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1)));

            var expectedFrom = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var expectedTo = new DateTime(2025, 1, 1, 23, 59, 0, DateTimeKind.Utc);
            _bookingsDocumentStore.Verify(b => b.GetInDateRangeAsync(expectedFrom, expectedTo, MockSite));

            _bookingsDocumentStore.Verify(
                x => x.UpdateStatus(It.IsAny<string>(), It.IsAny<AppointmentStatus>(),
                    It.IsAny<AvailabilityStatus>()),
                Times.Never);
        }

        [Fact]
        public async Task RecalculateAppointmentStatuses_SchedulesOrphanedAppointmentsIfPossible()
        {
            IEnumerable<Booking> bookings = new List<Booking>
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

            _bookingsDocumentStore
                .Setup(x => x.GetInDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), MockSite))
                .ReturnsAsync(bookings);

            var sessions = new List<SessionInstance>
            {
                new(new DateTime(2025, 01, 01, 9, 0, 0), new DateTime(2025, 01, 1, 12, 0, 0))
                {
                    Services = ["Service 1"], SlotLength = 10, Capacity = 1
                }
            };

            _availabilityStore
                .Setup(x => x.GetSessions(
                    It.Is<string>(s => s == MockSite),
                    It.IsAny<DateOnly>(),
                    It.IsAny<DateOnly>()))
                .ReturnsAsync(sessions);

            await _bookingWriteService.RecalculateAppointmentStatuses(MockSite, new DateOnly(2025, 1, 1));

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
            IEnumerable<Booking> bookings = new List<Booking>
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

            _bookingsDocumentStore
                .Setup(x => x.GetInDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), MockSite))
                .ReturnsAsync(bookings);

            var sessions = new List<SessionInstance>
            {
                new(new DateTime(2025, 01, 01, 10, 0, 0), new DateTime(2025, 01, 1, 12, 0, 0))
                {
                    Services = ["Service 1"], SlotLength = 10, Capacity = 1
                }
            };

            _availabilityStore
                .Setup(x => x.GetSessions(
                    It.Is<string>(s => s == MockSite),
                    It.IsAny<DateOnly>(),
                    It.IsAny<DateOnly>()))
                .ReturnsAsync(sessions);

            await _bookingWriteService.RecalculateAppointmentStatuses(MockSite, new DateOnly(2025, 1, 1));

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
            IEnumerable<Booking> bookings = new List<Booking>
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
                    Status = AppointmentStatus.Provisional,
                    Duration = 10,
                    Site = "mock-site",
                    AvailabilityStatus = AvailabilityStatus.Supported,
                }
            };

            _bookingsDocumentStore
                .Setup(x => x.GetInDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), MockSite))
                .ReturnsAsync(bookings);

            var sessions = new List<SessionInstance>
            {
                new(new DateTime(2025, 01, 01, 10, 0, 0), new DateTime(2025, 01, 1, 12, 0, 0))
                {
                    Services = ["Service 1"], SlotLength = 10, Capacity = 1
                }
            };

            _availabilityStore
                .Setup(x => x.GetSessions(
                    It.Is<string>(s => s == MockSite),
                    It.IsAny<DateOnly>(),
                    It.IsAny<DateOnly>()))
                .ReturnsAsync(sessions);

            await _bookingWriteService.RecalculateAppointmentStatuses(MockSite, new DateOnly(2025, 1, 1));

            _bookingsDocumentStore.Verify(x => x.UpdateAvailabilityStatus(
                    It.Is<string>(s => s == "1"),
                    It.Is<AvailabilityStatus>(s => s == AvailabilityStatus.Orphaned)),
                Times.Once);

            _bookingsDocumentStore.Verify(x => x.DeleteBooking(
                    It.Is<string>(s => s == "2"),
                    It.Is<string>(s => s == "mock-site")),
                Times.Once);
        }

        [Fact]
        public async Task RecalculateAppointmentStatuses_PrioritisesAppointmentsByCreatedDate()
        {
            IEnumerable<Booking> bookings = new List<Booking>
            {
                new()
                {
                    From = new DateTime(2025, 01, 01, 9, 0, 0),
                    Reference = "1",
                    AttendeeDetails = new AttendeeDetails { FirstName = "Daniel", LastName = "Dixon" },
                    Status = AppointmentStatus.Booked,
                    AvailabilityStatus = AvailabilityStatus.Orphaned,
                    Service = "Service 1",
                    Duration = 10,
                    Created = new DateTime(2024, 12, 01, 12, 0, 0)
                },
                new()
                {
                    From = new DateTime(2025, 01, 01, 9, 0, 0),
                    Reference = "2",
                    AttendeeDetails = new AttendeeDetails { FirstName = "Alexander", LastName = "Cooper" },
                    Status = AppointmentStatus.Booked,
                    AvailabilityStatus = AvailabilityStatus.Orphaned,
                    Service = "Service 1",
                    Duration = 10,
                    Created = new DateTime(2024, 11, 01, 12, 0, 0)
                }
            };

            _bookingsDocumentStore
                .Setup(x => x.GetInDateRangeAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), MockSite))
                .ReturnsAsync(bookings);

            var sessions = new List<SessionInstance>
            {
                new(new DateTime(2025, 01, 01, 9, 0, 0), new DateTime(2025, 01, 1, 12, 0, 0))
                {
                    Services = ["Service 1"], SlotLength = 10, Capacity = 1
                }
            };

            _availabilityStore
                .Setup(x => x.GetSessions(
                    It.Is<string>(s => s == MockSite),
                    It.IsAny<DateOnly>(),
                    It.IsAny<DateOnly>()))
                .ReturnsAsync(sessions);

            await _bookingWriteService.RecalculateAppointmentStatuses(MockSite, new DateOnly(2025, 1, 1));

            _bookingsDocumentStore.Verify(x => x.UpdateAvailabilityStatus(
                    It.Is<string>(s => s == "1"),
                    It.Is<AvailabilityStatus>(s => s == AvailabilityStatus.Supported)),
                Times.Never);

            _bookingsDocumentStore.Verify(x => x.UpdateAvailabilityStatus(
                    It.Is<string>(s => s == "2"),
                    It.Is<AvailabilityStatus>(s => s == AvailabilityStatus.Supported)),
                Times.Once);
        }
    }

    /// <summary>
    ///     Test suite for MultipleServices flag disabled
    /// </summary>
    [MockedFeatureToggle(Flags.MultipleServices, false)]
    public class BookingWriteServiceTests_SingleService()
        : BookingWriteBaseServiceTests(typeof(BookingWriteServiceTests_SingleService));

    /// <summary>
    ///     Test suite for MultipleServices flag enabled
    /// </summary>
    [MockedFeatureToggle(Flags.MultipleServices, true)]
    public class BookingWriteServiceTests_MultipleServices() : BookingWriteBaseServiceTests(
        typeof(BookingWriteServiceTests_MultipleServices));

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
        }
    }
}
