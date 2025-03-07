using Nhs.Appointments.Core.Concurrency;
using Nhs.Appointments.Core.Messaging;

namespace Nhs.Appointments.Core.UnitTests;

public class AvailabilityServiceTests
{
    private readonly AvailabilityService _sut;
    private readonly Mock<IAvailabilityStore> _availabilityStore = new();
    private readonly Mock<IAvailabilityCreatedEventStore> _availabilityCreatedEventStore = new();
    private readonly Mock<IBookingsService> _bookingsService = new();
    private readonly Mock<ISiteLeaseManager> _siteLeaseManager = new();
    private readonly Mock<IBookingsDocumentStore> _bookingsDocumentStore = new();
    private readonly Mock<IReferenceNumberProvider> _referenceNumberProvider = new();
    private readonly Mock<IBookingEventFactory> _eventFactory = new();
    private readonly Mock<IMessageBus> _messageBus = new();
    private readonly Mock<TimeProvider> _time = new();


    public AvailabilityServiceTests() => _sut = new AvailabilityService(_availabilityStore.Object,
        _availabilityCreatedEventStore.Object, _bookingsService.Object, _siteLeaseManager.Object,
        _bookingsDocumentStore.Object, _referenceNumberProvider.Object, _eventFactory.Object, _messageBus.Object,
        _time.Object);

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ApplyTemplateAsync_ThrowsArgumentException_IfSiteIsEmpty(string siteId)
    {
        var site = siteId;
        const string user = "mock.user@nhs.net";
        var from = new DateOnly(2025, 01, 06);
        var until = new DateOnly(2025, 01, 12);
        var template = new Template()
        {
            Days = [DayOfWeek.Monday],
            Sessions =
            [
                new Session()
                {
                    Capacity = 1,
                    From = new TimeOnly(09, 00),
                    Until = new TimeOnly(10, 00),
                    SlotLength = 5,
                    Services = ["Service 1"]
                }
            ]
        };
        var applyTemplate = async () => { await _sut.ApplyAvailabilityTemplateAsync(site, from, until, template, ApplyAvailabilityMode.Overwrite, user); };
        await applyTemplate.Should().ThrowAsync<ArgumentException>().WithMessage("site must have a value");
    }
    
    [Fact]
    public async Task ApplyTemplateAsync_ThrowsArgumentException_IfTemplateIsNull()
    {
        const string site = "2de5bb57-060f-4cb5-b14d-16587d0c2e8f";
        const string user = "mock.user@nhs.net";
        var from = new DateOnly(2025, 01, 06);
        var until = new DateOnly(2025, 01, 12);
        Template template = null;

        var applyTemplate = async () => { await _sut.ApplyAvailabilityTemplateAsync(site, from, until, template, ApplyAvailabilityMode.Overwrite, user); };
        await applyTemplate.Should().ThrowAsync<ArgumentException>().WithMessage("template must be provided");
    }
    
    [Fact]
    public async Task ApplyTemplateAsync_ThrowsArgumentException_IfFromDateIsAfterUntilDate()
    {
        const string site = "2de5bb57-060f-4cb5-b14d-16587d0c2e8f";
        const string user = "mock.user@nhs.net";
        var from = new DateOnly(2025, 01, 02);
        var until = new DateOnly(2025, 01, 01);
        var template = new Template()
        {
            Days = [DayOfWeek.Monday],
            Sessions =
            [
                new Session()
                {
                    Capacity = 1,
                    From = new TimeOnly(09, 00),
                    Until = new TimeOnly(10, 00),
                    SlotLength = 5,
                    Services = ["Service 1"]
                }
            ]
        };
        var applyTemplate = async () => { await _sut.ApplyAvailabilityTemplateAsync(site, from, until, template, ApplyAvailabilityMode.Overwrite, user); };
        await applyTemplate.Should().ThrowAsync<ArgumentException>().WithMessage("until date must be after from date");
    }
    
    [Fact]
    public async Task ApplyTemplateAsync_ThrowsArgumentException_IfNoDaysAreSpecified()
    {
        const string site = "2de5bb57-060f-4cb5-b14d-16587d0c2e8f";
        const string user = "mock.user@nhs.net";
        var from = new DateOnly(2025, 01, 06);
        var until = new DateOnly(2025, 01, 12);
        var template = new Template()
        {
            Days = [],
            Sessions =
            [
                new Session()
                {
                    Capacity = 1,
                    From = new TimeOnly(09, 00),
                    Until = new TimeOnly(10, 00),
                    SlotLength = 5,
                    Services = ["Service 1"]
                }
            ]
        };
        var applyTemplate = async () => { await _sut.ApplyAvailabilityTemplateAsync(site, from, until, template, ApplyAvailabilityMode.Overwrite, user); };
        await applyTemplate.Should().ThrowAsync<ArgumentException>("template must specify one or more weekdays");
    }
    
    [Fact]
    public async Task ApplyTemplateAsync_ThrowsArgumentException_IfTemplateContainsNoSessions()
    {
        const string site = "2de5bb57-060f-4cb5-b14d-16587d0c2e8f";
        const string user = "mock.user@nhs.net";
        var from = new DateOnly(2025, 01, 06);
        var until = new DateOnly(2025, 01, 12);
        var template = new Template()
        {
            Days = [DayOfWeek.Monday],
            Sessions = []
        };
        var applyTemplate = async () => { await _sut.ApplyAvailabilityTemplateAsync(site, from, until, template, ApplyAvailabilityMode.Overwrite, user); };
        await applyTemplate.Should().ThrowAsync<ArgumentException>("template must contain one or more sessions");
    }
    
    [Fact]
    public async Task ApplyTemplateAsync_CallsAvailabilityStore_WithDatesForRequestedDays()
    {
        const string site = "2de5bb57-060f-4cb5-b14d-16587d0c2e8f";
        const string user = "mock.user@nhs.net";
        var from = new DateOnly(2025, 01, 06);
        var until = new DateOnly(2025, 01, 12);
        Session[] sessions =
        [
            new ()
            {
                Capacity = 1,
                From = new TimeOnly(09, 00),
                Until = new TimeOnly(10, 00),
                SlotLength = 5,
                Services = ["Service 1"]
            }
        ];

        var template = new Template()
        {
            Days = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Saturday, DayOfWeek.Sunday],
            Sessions = sessions
        };
        
        await _sut.ApplyAvailabilityTemplateAsync(site, from, until, template, ApplyAvailabilityMode.Overwrite, user);
        var actualDates = _availabilityStore.Invocations
            .Where(i => i.Method.Name == nameof(IAvailabilityStore.ApplyAvailabilityTemplate))
            .Select(i => (DateOnly)i.Arguments[1]);
        var expectedDates = new[]
        {
            new DateOnly(2025, 01, 06),
            new DateOnly(2025, 01, 07),
            new DateOnly(2025, 01, 11),
            new DateOnly(2025, 01, 12)
        };
        actualDates.Should().BeEquivalentTo(expectedDates);

        _availabilityCreatedEventStore.Verify(x => x.LogTemplateCreated(site, from, until, template, user), Times.Once);
    }

    [Fact]
    public async Task ApplyTemplateAsync_CallsBookingServiceToRecalculateAppointmentStatuses_WithDatesForRequestedDays()
    {
        const string site = "2de5bb57-060f-4cb5-b14d-16587d0c2e8f";
        const string user = "mock.user@nhs.net";
        var from = new DateOnly(2025, 01, 06);
        var until = new DateOnly(2025, 01, 8);
        Session[] sessions =
        [
            new()
            {
                Capacity = 1,
                From = new TimeOnly(09, 00),
                Until = new TimeOnly(10, 00),
                SlotLength = 5,
                Services = ["Service 1"]
            }
        ];

        var template = new Template
        {
            Days =
            [
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday,
                DayOfWeek.Saturday, DayOfWeek.Sunday
            ],
            Sessions = sessions
        };

        await _sut.ApplyAvailabilityTemplateAsync(site, from, until, template, ApplyAvailabilityMode.Overwrite, user);

        _bookingsService.Verify(x => x.RecalculateAppointmentStatuses(site, new DateOnly(2025, 01, 06)), Times.Once);
        _bookingsService.Verify(x => x.RecalculateAppointmentStatuses(site, new DateOnly(2025, 01, 07)), Times.Once);
        _bookingsService.Verify(x => x.RecalculateAppointmentStatuses(site, new DateOnly(2025, 01, 08)), Times.Once);
    }

    [Fact]
    public async Task ApplySingleDateSessionAsync_CallsAvailabilityCreatedEventStore()
    {
        const string site = "2de5bb57-060f-4cb5-b14d-16587d0c2e8f";
        const string user = "mock.user@nhs.net";
        var date = new DateOnly(2024, 10, 10);

        var sessions = new List<Session>
        {
            new()
            {
                Capacity = 1,
                From = TimeOnly.FromDateTime(new DateTime(2024, 10, 10, 9, 0, 0)),
                Until = TimeOnly.FromDateTime(new DateTime(2024, 10, 10, 16, 0, 0)),
                Services = ["RSV", "COVID"],
                SlotLength = 5
            }
        }.ToArray();

        await _sut.ApplySingleDateSessionAsync(date, site, sessions, ApplyAvailabilityMode.Overwrite, user);

        _availabilityStore.Verify(
            x => x.ApplyAvailabilityTemplate(site, date, sessions, ApplyAvailabilityMode.Overwrite, null), Times.Once);
        _availabilityCreatedEventStore.Verify(x => x.LogSingleDateSessionCreated(site, date, sessions, user), Times.Once);
    }

    [Fact]
    public async Task ApplySingleDateSessionAsync_CallsBookingServiceToRecalculateAppointmentStatuses()
    {
        const string site = "2de5bb57-060f-4cb5-b14d-16587d0c2e8f";
        const string user = "mock.user@nhs.net";
        var date = new DateOnly(2024, 10, 10);

        var sessions = new List<Session>
        {
            new()
            {
                Capacity = 1,
                From = TimeOnly.FromDateTime(new DateTime(2024, 10, 10, 9, 0, 0)),
                Until = TimeOnly.FromDateTime(new DateTime(2024, 10, 10, 16, 0, 0)),
                Services = ["RSV", "COVID"],
                SlotLength = 5
            }
        }.ToArray();

        await _sut.ApplySingleDateSessionAsync(date, site, sessions, ApplyAvailabilityMode.Overwrite, user);

        _bookingsService.Verify(x => x.RecalculateAppointmentStatuses(site, new DateOnly(2024, 10, 10)), Times.Once);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("")]
    [InlineData(null)]
    public async Task SetAvailabilityAsync_ThrowsArgumentException_WhenSiteIsInvalid(string site)
    {
        var setAvailability = async () => { await _sut.SetAvailabilityAsync(new DateOnly(2024, 10, 10), site, Array.Empty<Session>(), ApplyAvailabilityMode.Overwrite); };
        await setAvailability.Should().ThrowAsync<ArgumentException>("Site must have a value.");
    }

    [Fact]
    public async Task SetAvailabilityAsync_ThrowsArgumentException_WhenSessionsArrayIsEmpty()
    {
        var setAvailability = async () => { await _sut.SetAvailabilityAsync(new DateOnly(2024, 10, 10), "test-site", Array.Empty<Session>(), ApplyAvailabilityMode.Overwrite); };
        await setAvailability.Should().ThrowAsync<ArgumentException>("Availability must contain one or more sessions.");
    }

    [Fact]
    public async Task SetAvailability_CallsAvailabilityStore_WithSessions()
    {
        var sessions = new List<Session>
        {
            new()
                {
                    Capacity = 1,
                    From = TimeOnly.FromDateTime(new DateTime(2024, 10, 10, 9, 0, 0)),
                    Until = TimeOnly.FromDateTime(new DateTime(2024, 10, 10, 16, 0, 0)),
                    Services = ["RSV", "COVID"],
                    SlotLength = 5
                }
        }.ToArray();
        var date = new DateOnly(2024, 10, 10);
        var site = "test-site";

        await _sut.SetAvailabilityAsync(date, site, sessions, ApplyAvailabilityMode.Overwrite);

        _availabilityStore.Verify(
            x => x.ApplyAvailabilityTemplate(site, date, sessions, ApplyAvailabilityMode.Overwrite, null), Times.Once);
    }

    [Fact]
    public async Task SetAvailability_CanEditSessions()
    {
        var sessions = new List<Session>
        {
            new()
            {
                Capacity = 1,
                From = TimeOnly.FromDateTime(new DateTime(2024, 10, 10, 9, 0, 0)),
                Until = TimeOnly.FromDateTime(new DateTime(2024, 10, 10, 16, 0, 0)),
                Services = ["RSV", "COVID"],
                SlotLength = 5
            }
        }.ToArray();

        var sessionToEdit = new Session
        {
            Capacity = 1,
            From = TimeOnly.FromDateTime(new DateTime(2024, 10, 10, 9, 0, 0)),
            Until = TimeOnly.FromDateTime(new DateTime(2024, 10, 10, 16, 0, 0)),
            Services = ["RSV", "COVID"],
            SlotLength = 5
        };

        var date = new DateOnly(2024, 10, 10);
        var site = "test-site";

        await _sut.SetAvailabilityAsync(date, site, sessions, ApplyAvailabilityMode.Edit, sessionToEdit);

        _availabilityStore.Verify(
            x => x.ApplyAvailabilityTemplate(site, date, sessions, ApplyAvailabilityMode.Edit, sessionToEdit),
            Times.Once);
    }

    [Fact]
    public async Task SetAvailabilityAsync_ThrowsArgumentException_WhenModeIsEditButSessionToEditIsNull()
    {
        var sessions = new List<Session>
        {
            new()
            {
                Capacity = 1,
                From = TimeOnly.FromDateTime(new DateTime(2024, 10, 10, 9, 0, 0)),
                Until = TimeOnly.FromDateTime(new DateTime(2024, 10, 10, 16, 0, 0)),
                Services = ["RSV", "COVID"],
                SlotLength = 5
            }
        }.ToArray();

        var date = new DateOnly(2024, 10, 10);
        var site = "test-site";

        var setAvailability = async () =>
        {
            await _sut.SetAvailabilityAsync(date, site, sessions, ApplyAvailabilityMode.Edit);
        };
        await setAvailability.Should()
            .ThrowAsync<ArgumentException>("When editing a session a session to edit must be supplied.");
    }

    [Fact]
    public async Task GetAvailabilityCreatedEvents_OrdersEventsByFromThenByTo()
    {
        var availabilityCreatedEvents = new List<AvailabilityCreatedEvent>()
        {
            new()
            {
                Created = DateTime.UtcNow,
                By = "some.user@nhs.net",
                Site = "some-site",
                From = DateOnly.FromDateTime(new DateTime(2025, 4, 3)),
            },
            new()
            {
                Created = DateTime.UtcNow,
                By = "some.user@nhs.net",
                Site = "some-site",
                From = DateOnly.FromDateTime(new DateTime(2024, 10, 10)),
                To = DateOnly.FromDateTime(new DateTime(2024, 10, 20)),
            },
            new()
            {
                Created = DateTime.UtcNow,
                By = "some.user@nhs.net",
                Site = "some-site",
                From = DateOnly.FromDateTime(new DateTime(2024, 10, 10)),
                To = DateOnly.FromDateTime(new DateTime(2024, 10, 15)),
            }
        };

        _availabilityCreatedEventStore.Setup(x => x.GetAvailabilityCreatedEvents(It.IsAny<string>()))
            .ReturnsAsync(availabilityCreatedEvents);

        var fromDate = DateOnly.FromDateTime(DateTime.MinValue);
        var result = (await _sut.GetAvailabilityCreatedEventsAsync("some-site", fromDate)).ToList();

        result.Should().HaveCount(3);

        result[0].From.Should().Be(DateOnly.FromDateTime(new DateTime(2024, 10, 10)));
        result[0].To.Should().Be(DateOnly.FromDateTime(new DateTime(2024, 10, 15)));

        result[1].From.Should().Be(DateOnly.FromDateTime(new DateTime(2024, 10, 10)));
        result[1].To.Should().Be(DateOnly.FromDateTime(new DateTime(2024, 10, 20)));

        result[2].From.Should().Be(DateOnly.FromDateTime(new DateTime(2025, 4, 3)));
        result[2].To.Should().BeNull();
    }

    [Fact]
    public async Task GetAvailabilityCreatedEvents_FiltersEventsAfterDate()
    {
        var availabilityCreatedEvents = new List<AvailabilityCreatedEvent>()
        {
            new()
            {
                Created = DateTime.UtcNow,
                By = "some.user@nhs.net",
                Site = "some-site",
                From = DateOnly.FromDateTime(new DateTime(2025, 4, 3)),
            },
            new()
            {
                Created = DateTime.UtcNow,
                By = "some.user@nhs.net",
                Site = "some-site",
                From = DateOnly.FromDateTime(new DateTime(2024, 10, 10)),
                To = DateOnly.FromDateTime(new DateTime(2024, 10, 20)),
            },
            new()
            {
                Created = DateTime.UtcNow,
                By = "some.user@nhs.net",
                Site = "some-site",
                From = DateOnly.FromDateTime(new DateTime(2024, 10, 10)),
                To = DateOnly.FromDateTime(new DateTime(2024, 10, 15)),
            }
        };

        _availabilityCreatedEventStore.Setup(x => x.GetAvailabilityCreatedEvents(It.IsAny<string>()))
            .ReturnsAsync(availabilityCreatedEvents);

        var fromDate = DateOnly.FromDateTime(new DateTime(2024, 10, 17));
        var result = (await _sut.GetAvailabilityCreatedEventsAsync("some-site", fromDate)).ToList();

        result.Should().HaveCount(2);

        result[0].From.Should().Be(DateOnly.FromDateTime(new DateTime(2024, 10, 10)));
        result[0].To.Should().Be(DateOnly.FromDateTime(new DateTime(2024, 10, 20)));

        result[1].From.Should().Be(DateOnly.FromDateTime(new DateTime(2025, 4, 3)));
        result[1].To.Should().BeNull();
    }

    [Fact]
    public async Task GetDailyAvailabiltiy_ReturnsAvailabilityWithinDateRange()
    {
        var fromDate = DateOnly.FromDateTime(new DateTime(2024, 12, 1));
        var toDate = DateOnly.FromDateTime(new DateTime(2024, 12, 8));

        var availability = new List<DailyAvailability>
        {
            new()
            {
                Date = DateOnly.FromDateTime(new DateTime(2024, 12, 1)),
                Sessions =
                [
                    new()
                    {
                        From = TimeOnly.FromTimeSpan(TimeSpan.FromHours(11)),
                        Until = TimeOnly.FromTimeSpan(TimeSpan.FromHours(16)),
                        Capacity = 2,
                        SlotLength = 5,
                        Services = ["RSV:Adult"]
                    }
                ]
            },
            new()
            {
                Date = DateOnly.FromDateTime(new DateTime(2024, 12, 4)),
                Sessions =
                [
                    new()
                    {
                        From = TimeOnly.FromTimeSpan(TimeSpan.FromHours(11)),
                        Until = TimeOnly.FromTimeSpan(TimeSpan.FromHours(16)),
                        Capacity = 2,
                        SlotLength = 5,
                        Services = ["RSV:Adult"]
                    }
                ]
            }
        };

        _availabilityStore.Setup(x => x.GetDailyAvailability(It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(availability);

        var result = await _sut.GetDailyAvailability("TEST01", fromDate, toDate);

        result.Any().Should().BeTrue();
        result.Count().Should().Be(2);
    }

    [Fact]
    public async Task CancelSession_CallsAvailabilityStore()
    {
        var site = "TEST01";
        var date = new DateOnly(2025, 1, 10);

        await _sut.CancelSession(site, date, "09:00", "12:00", ["RSV:Adult"], 5, 2);

        _availabilityStore.Verify(x => x.CancelSession(site, date, It.IsAny<Session>()), Times.Once());
    }

    public class GetAvailabilityStateTests
    {
        private readonly AvailabilityService _sut;
        private readonly Mock<IAvailabilityStore> _availabilityStore = new();
        private readonly Mock<IAvailabilityCreatedEventStore> _availabilityCreatedEventStore = new();
        private readonly Mock<IBookingsService> _bookingsService = new();
        private readonly Mock<ISiteLeaseManager> _siteLeaseManager = new();
        private readonly Mock<IBookingsDocumentStore> _bookingsDocumentStore = new();
        private readonly Mock<IReferenceNumberProvider> _referenceNumberProvider = new();
        private readonly Mock<IBookingEventFactory> _eventFactory = new();
        private readonly Mock<IMessageBus> _messageBus = new();
        private readonly Mock<TimeProvider> _time = new();

        private const string MockSite = "some-site";

        public GetAvailabilityStateTests() => _sut = new AvailabilityService(_availabilityStore.Object,
            _availabilityCreatedEventStore.Object, _bookingsService.Object, _siteLeaseManager.Object,
            _bookingsDocumentStore.Object, _referenceNumberProvider.Object, _eventFactory.Object, _messageBus.Object,
            _time.Object);

        private DateTime TestDateAt(string time)
        {
            var hour = int.Parse(time.Split(":")[0]);
            var minute = int.Parse(time.Split(":")[1]);
            return new DateTime(2025, 01, 01, hour, minute, 0);
        }

        private Booking TestBooking(string reference, string service, string from = "09:00",
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

        private SessionInstance TestSession(string start, string end, string[] services, int slotLength = 10,
            int capacity = 1) =>
            new(TestDateAt(start), TestDateAt(end))
            {
                Services = services, SlotLength = slotLength, Capacity = capacity
            };

        private void SetupAvailabilityAndBookings(List<Booking> bookings, List<SessionInstance> sessions)
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

        [Fact]
        public async Task MakesNoChangesIfAllAppointmentsAreStillValid()
        {
            var bookings = new List<Booking>
            {
                TestBooking("1", "Green", "09:10", avStatus: "Supported"),
                TestBooking("2", "Green", "09:20", avStatus: "Supported"),
                TestBooking("3", "Green", "09:30", avStatus: "Supported")
            };

            var sessions = new List<SessionInstance> { TestSession("09:00", "12:00", ["Green"]) };

            SetupAvailabilityAndBookings(bookings, sessions);

            var resultingAvailabilityState = await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1));

            resultingAvailabilityState.Recalculations.Should().BeEmpty();
            resultingAvailabilityState.Bookings.Should().BeEquivalentTo(bookings);
            resultingAvailabilityState.AvailableSlots.Should().HaveCount(15);
        }

        [Fact]
        public async Task SchedulesOrphanedAppointmentsIfPossible()
        {
            var bookings = new List<Booking>
            {
                TestBooking("1", "Green", "09:10"),
                TestBooking("2", "Green", "09:20"),
                TestBooking("3", "Green", "09:30")
            };

            var sessions = new List<SessionInstance> { TestSession("09:00", "12:00", ["Green"]) };

            SetupAvailabilityAndBookings(bookings, sessions);

            var resultingAvailabilityState = await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1));

            resultingAvailabilityState.Recalculations.Should().HaveCount(3);
            resultingAvailabilityState.Bookings.Should().BeEquivalentTo(bookings);
            resultingAvailabilityState.AvailableSlots.Should().HaveCount(15);
        }

        [Fact]
        public async Task OrphansLiveAppointmentsIfTheyCannotBeFulfilled()
        {
            var bookings = new List<Booking>
            {
                TestBooking("1", "Green", "09:10", avStatus: "Supported", creationOrder: 1),
                TestBooking("2", "Green", "09:10", avStatus: "Supported", creationOrder: 2)
            };

            var sessions = new List<SessionInstance> { TestSession("09:00", "12:00", ["Green"]) };

            SetupAvailabilityAndBookings(bookings, sessions);

            var resultingAvailabilityState = await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1));

            resultingAvailabilityState.Recalculations.Should().ContainSingle(s =>
                s.Booking.Reference == "2" && s.Action == AvailabilityUpdateAction.SetToOrphaned);
            resultingAvailabilityState.Bookings.Should().HaveCount(1);
            resultingAvailabilityState.AvailableSlots.Should().HaveCount(17);
        }

        [Fact]
        public async Task DeletesProvisionalAppointments()
        {
            var bookings = new List<Booking>
            {
                TestBooking("1", "Green", avStatus: "Supported", status: "Booked", creationOrder: 1),
                TestBooking("2", "Green", "09:10", status: "Provisional", creationOrder: 2)
            };

            var sessions = new List<SessionInstance> { TestSession("10:00", "12:00", ["Green"]) };

            SetupAvailabilityAndBookings(bookings, sessions);

            var resultingAvailabilityState = await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1));

            resultingAvailabilityState.Recalculations.Should().Contain(s =>
                s.Booking.Reference == "1" && s.Action == AvailabilityUpdateAction.SetToOrphaned);
            resultingAvailabilityState.Recalculations.Should().Contain(s =>
                s.Booking.Reference == "2" && s.Action == AvailabilityUpdateAction.ProvisionalToDelete);
            resultingAvailabilityState.Bookings.Should().BeEmpty();
            resultingAvailabilityState.AvailableSlots.Should().HaveCount(12);
        }

        [Fact]
        public async Task PrioritisesAppointmentsByCreatedDate()
        {
            var bookings = new List<Booking>
            {
                TestBooking("1", "Green", "09:30", creationOrder: 3),
                TestBooking("2", "Green", "09:30", creationOrder: 1),
                TestBooking("3", "Green", "09:30", creationOrder: 2)
            };

            var sessions = new List<SessionInstance> { TestSession("09:00", "12:00", ["Green"]) };

            SetupAvailabilityAndBookings(bookings, sessions);

            var resultingAvailabilityState = await _sut.GetAvailabilityState(MockSite, new DateOnly(2025, 1, 1));

            resultingAvailabilityState.Bookings.Should().ContainSingle(b => b.Reference == "2");
            resultingAvailabilityState.Recalculations.Should().ContainSingle(r =>
                r.Booking.Reference == "2" && r.Action == AvailabilityUpdateAction.SetToSupported);
            resultingAvailabilityState.AvailableSlots.Should().HaveCount(17);
        }
    }
}
