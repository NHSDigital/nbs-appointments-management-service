using Nhs.Appointments.Core.Concurrency;
using Nhs.Appointments.Core.Messaging;

namespace Nhs.Appointments.Core.UnitTests;

[MockedFeatureToggle("MultiServiceAvailabilityCalculations", false)]
public class AvailabilityServiceTests : FeatureToggledTests
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

    public AvailabilityServiceTests() : base(typeof(AvailabilityServiceTests)) =>
        _sut = new AvailabilityService(_availabilityStore.Object,
            _availabilityCreatedEventStore.Object, _bookingsService.Object, _siteLeaseManager.Object,
            _bookingsDocumentStore.Object, _referenceNumberProvider.Object, _eventFactory.Object, _messageBus.Object,
            _time.Object, _featureToggleHelper.Object);

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
}
