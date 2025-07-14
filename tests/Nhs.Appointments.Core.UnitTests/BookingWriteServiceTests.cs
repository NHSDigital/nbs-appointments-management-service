using Nhs.Appointments.Core.Concurrency;
using Nhs.Appointments.Core.Features;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Messaging.Events;

#pragma warning disable CS0618 // Keep availabilityCalculator around until MultipleServicesEnabled is stable

namespace Nhs.Appointments.Core.UnitTests
{
    public abstract class BookingWriteBaseServiceTests : FeatureToggledTests
    {
        protected const string MockSite = "some-site";

        protected readonly Mock<IBookingAvailabilityStateService> _bookingAvailabilityStateService = new();
        protected readonly Mock<IAvailabilityCalculator> _availabilityCalculator = new();
        protected readonly Mock<IAvailabilityCreatedEventStore> _availabilityCreatedEventStore = new();
        protected readonly Mock<IAvailabilityStore> _availabilityStore = new();

        protected readonly Mock<IBookingQueryService> _bookingQueryService = new();
        protected readonly Mock<IBookingsDocumentStore> _bookingsDocumentStore = new();
        protected readonly Mock<IMessageBus> _messageBus = new();
        protected readonly Mock<IReferenceNumberProvider> _referenceNumberProvider = new();
        protected readonly Mock<ISiteLeaseManager> _siteLeaseManager = new();
        protected BookingWriteService _sut;

        protected BookingWriteBaseServiceTests(Type testClassType) : base(testClassType)
        {
            _sut = new BookingWriteService(
                _bookingsDocumentStore.Object,
                _bookingQueryService.Object,
                _referenceNumberProvider.Object,
                _siteLeaseManager.Object,
                _availabilityStore.Object,
                _availabilityCalculator.Object,
                _bookingAvailabilityStateService.Object,
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
            SessionInstance[] availability =
            [
                new(new DateTime(2077, 1, 1, 9, 0, 0, 0), new DateTime(2077, 1, 1, 12, 0, 0, 0))
            ];

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
                new BookingAvailabilityStateService(availabilityQueryService, bookingQueryService),
                new EventFactory(), _messageBus.Object, TimeProvider.System, _featureToggleHelper.Object);

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

            MockAvailabilityForSingleServiceAndMultipleService(availability);

            _referenceNumberProvider.Setup(x => x.GetReferenceNumber(It.IsAny<string>())).ReturnsAsync("TEST1");

            var result = await _sut.MakeBooking(booking);

            _messageBus.Verify(x => x.Send(It.Is<BookingMade>(e => e.Reference == booking.Reference)));
        }

        private void MockAvailabilityForSingleServiceAndMultipleService(SessionInstance[] availability)
        {
            //mock for SingleService
            _availabilityCalculator.Setup(x =>
                    x.CalculateAvailability(MockSite, "TSERV", new DateOnly(2077, 1, 1), new DateOnly(2077, 1, 2)))
                .ReturnsAsync(availability);

            //mock for MultiService
            _bookingAvailabilityStateService.Setup(x => x.GetAvailableSlots(MockSite, new DateTime(2077, 1, 1, 10, 0, 0, 0),
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

            MockAvailabilityForSingleServiceAndMultipleService(availability);

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

            //mock for SingleService
            _availabilityCalculator.Setup(x =>
                    x.CalculateAvailability(MockSite, "TSERV", new DateOnly(2077, 1, 1), new DateOnly(2077, 1, 2)))
                .ReturnsAsync([
                    new SessionInstance(new DateTime(2077, 1, 1, 10, 0, 0, 0), new DateTime(2077, 1, 1, 10, 10, 0, 0))
                    {
                        Services = ["TSERV"]
                    },
                    new SessionInstance(new DateTime(2077, 1, 1, 11, 0, 0, 0), new DateTime(2077, 1, 1, 11, 10, 0, 0))
                    {
                        Services = ["TSERV"]
                    }
                ]);

            //mock for MultiService
            _bookingAvailabilityStateService.Setup(x => x.GetAvailableSlots(MockSite, new DateTime(2077, 1, 1, 10, 0, 0, 0),
                    new DateTime(2077, 1, 1, 10, 10, 0, 0)))
                .ReturnsAsync([
                        new SessionInstance(new DateTime(2077, 1, 1, 10, 0, 0, 0),
                            new DateTime(2077, 1, 1, 10, 10, 0, 0)) { Services = ["TSERV"] }
                    ]);

            //mock for MultiService
            _bookingAvailabilityStateService.Setup(x => x.GetAvailableSlots(MockSite, new DateTime(2077, 1, 1, 11, 0, 0, 0),
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

            //mock for SingleService
            _availabilityCalculator.Setup(x =>
                    x.CalculateAvailability(MockSite, "TSERV", new DateOnly(2077, 1, 1), new DateOnly(2077, 1, 2)))
                .ReturnsAsync([
                    new SessionInstance(new DateTime(2077, 1, 1, 10, 0, 0, 0), new DateTime(2077, 1, 1, 10, 10, 0, 0))
                    {
                        Services = ["TSERV"]
                    },
                    new SessionInstance(new DateTime(2077, 1, 1, 11, 0, 0, 0), new DateTime(2077, 1, 1, 11, 10, 0, 0))
                    {
                        Services = ["TSERV"]
                    }
                ]);

            //mock for MultiService
            _bookingAvailabilityStateService.Setup(x => x.GetAvailableSlots(MockSite, new DateTime(2077, 1, 1, 10, 0, 0, 0),
                    new DateTime(2077, 1, 1, 10, 10, 0, 0)))
                .ReturnsAsync(
                    [
                        new SessionInstance(new DateTime(2077, 1, 1, 10, 0, 0, 0),
                            new DateTime(2077, 1, 1, 10, 10, 0, 0)) { Services = ["TSERV"] }
                    ]);

            //mock for MultiService
            _bookingAvailabilityStateService.Setup(x => x.GetAvailableSlots(MockSite, new DateTime(2077, 1, 1, 11, 0, 0, 0),
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

            MockAvailabilityForSingleServiceAndMultipleService(availability);

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
           
            MockAvailabilityForSingleServiceAndMultipleService(availability);
            
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

            //mock for SingleService
            _availabilityCalculator.Setup(x =>
                    x.CalculateAvailability(MockSite, "TSERV", new DateOnly(2077, 1, 1), new DateOnly(2077, 1, 2)))
                .ReturnsAsync(
                [
                    new SessionInstance(new DateTime(2077, 1, 1, 9, 0, 0, 0), new DateTime(2077, 1, 1, 12, 0, 0, 0))
                    {
                        Services = ["TSERV"]
                    }
                ]);

            //mock for MultiService
            _bookingAvailabilityStateService.Setup(x => x.GetAvailableSlots(MockSite, new DateTime(2077, 1, 1, 13, 0, 0, 0),
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
                .Setup(x => x.UpdateStatus(bookingRef, AppointmentStatus.Cancelled, AvailabilityStatus.Unknown, CancellationReason.CancelledByCitizen))
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
    }

    /// <summary>
    ///     Test suite for MultipleServices flag disabled
    /// </summary>
    [MockedFeatureToggle(Flags.MultipleServices, false)]
    public class BookingWriteServiceTests_SingleService()
        : BookingWriteBaseServiceTests(typeof(BookingWriteServiceTests_SingleService))
    {
        [Fact]
        public async Task RecalculateAppointmentStatuses_DoesntGoDownMultipleServiceCodePath()
        {
            _bookingAvailabilityStateService
                .Setup(x => x.BuildRecalculations(MockSite, It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(
                    new List<BookingAvailabilityUpdate>());

            await _sut.RecalculateAppointmentStatuses(MockSite, new DateOnly(2025, 1, 1));
            
            //singleService code path
            _bookingQueryService.Verify(x => x.GetBookings(
                    It.IsAny<DateTime>(), It.IsAny<DateTime>(), MockSite),
                Times.Once);
            _availabilityStore.Verify(x => x.GetSessions(
                    MockSite, It.IsAny<DateOnly>(), It.IsAny<DateOnly>()),
                Times.Once);
            
            //multiService code path
            _bookingAvailabilityStateService.Verify(x => x.BuildRecalculations(
                    MockSite, It.IsAny<DateTime>(), It.IsAny<DateTime>()),
                Times.Never);
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

            _bookingQueryService
                .Setup(x => x.GetBookings(It.IsAny<DateTime>(), It.IsAny<DateTime>(), MockSite))
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

            _bookingQueryService
                .Setup(x => x.GetBookings(It.IsAny<DateTime>(), It.IsAny<DateTime>(), MockSite))
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
        public async Task RecalculateAppointmentStatuses_PrioritisesAppointmentsByCreatedDate()
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

            _bookingQueryService
                .Setup(x => x.GetBookings(It.IsAny<DateTime>(), It.IsAny<DateTime>(), MockSite))
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

            await _sut.RecalculateAppointmentStatuses(MockSite, new DateOnly(2025, 1, 1));

            _bookingsDocumentStore.Verify(x => x.UpdateAvailabilityStatus(
                    It.Is<string>(s => s == "1"),
                    It.Is<AvailabilityStatus>(s => s == AvailabilityStatus.Supported)),
                Times.Never);

            _bookingsDocumentStore.Verify(x => x.UpdateAvailabilityStatus(
                    It.Is<string>(s => s == "2"),
                    It.Is<AvailabilityStatus>(s => s == AvailabilityStatus.Supported)),
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

            _bookingQueryService
                .Setup(x => x.GetBookings(It.IsAny<DateTime>(), It.IsAny<DateTime>(), MockSite))
                .ReturnsAsync(bookings.ToList());

            var sessions = new List<SessionInstance>
            {
                new(new DateTime(2025, 01, 01, 10, 0, 0), new DateTime(2025, 01, 1, 12, 0, 0))
                {
                    Services = [service], SlotLength = 10, Capacity = 1
                }
            };

            _availabilityStore
                .Setup(x => x.GetSessions(
                    It.Is<string>(s => s == MockSite),
                    It.IsAny<DateOnly>(),
                    It.IsAny<DateOnly>()))
                .ReturnsAsync(sessions);

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

            _bookingQueryService
                .Setup(x => x.GetBookings(It.IsAny<DateTime>(), It.IsAny<DateTime>(), MockSite))
                .ReturnsAsync(bookings.ToList());

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
            
            await _sut.RecalculateAppointmentStatuses(MockSite, new DateOnly(2025, 1, 1));

            _availabilityStore.Verify(a =>
                a.GetSessions(MockSite, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 1)));

            var expectedFrom = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var expectedTo = new DateTime(2025, 1, 1, 23, 59, 0, DateTimeKind.Utc);

            _bookingQueryService.Verify(b => b.GetBookings(expectedFrom, expectedTo, MockSite));

            _bookingsDocumentStore.Verify(
                x => x.UpdateStatus(It.IsAny<string>(), It.IsAny<AppointmentStatus>(),
                    It.IsAny<AvailabilityStatus>(), It.IsAny<CancellationReason>()),
                Times.Never);
        }

        [Fact]
        public async Task MakeBooking_CallsAvailabilityCalculator_WhenBooking()
        {
            var expectedFrom = new DateOnly(2077, 1, 1);
            var expectedUntil = expectedFrom.AddDays(1);

            var booking = new Booking { Site = "TEST", Service = "TSERV", From = new DateTime(2077, 1, 1) };

            _siteLeaseManager.Setup(x => x.Acquire(It.IsAny<string>())).Returns(new FakeLeaseContext());
            var result = await _sut.MakeBooking(booking);

            _availabilityCalculator.Verify(x => x.CalculateAvailability("TEST", "TSERV", expectedFrom, expectedUntil));
        }

        [Theory]
        [InlineData(CancellationReason.CancelledByCitizen, CancellationReason.CancelledByCitizen)]
        [InlineData(CancellationReason.CancelledBySite, CancellationReason.CancelledBySite)]
        public async Task CancelBooking_ValidCancellationReasonIsUsed(CancellationReason reason, CancellationReason expectedReason)
        {
            var reference = "BOOK-123";
            var site = "SITE01";
            var booking = new Booking
            {
                Reference = reference,
                Site = site,
                From = DateTime.UtcNow,
                Status = AppointmentStatus.Booked
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
    }

    /// <summary>
    ///     Test suite for MultipleServices flag enabled
    /// </summary>
    [MockedFeatureToggle(Flags.MultipleServices, true)]
    public class BookingWriteServiceTests_MultipleServices()
        : BookingWriteBaseServiceTests(typeof(BookingWriteServiceTests_MultipleServices))
    {
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
                    new List<BookingAvailabilityUpdate>()
                    {
                        new (bookings.First(), AvailabilityUpdateAction.SetToSupported),
                        new (bookings.Last(), AvailabilityUpdateAction.SetToSupported),
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
                    new List<BookingAvailabilityUpdate>()
                    {
                        new (bookings.First(), AvailabilityUpdateAction.SetToOrphaned),
                        new (bookings.Last(), AvailabilityUpdateAction.SetToOrphaned),
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
                    new List<BookingAvailabilityUpdate>()
                    {
                        new (bookings.First(), AvailabilityUpdateAction.SetToOrphaned),
                        new (bookings.Last(), AvailabilityUpdateAction.ProvisionalToDelete),
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
            _bookingAvailabilityStateService.Setup(x => x.BuildRecalculations(MockSite, It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(new List<BookingAvailabilityUpdate>());

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
