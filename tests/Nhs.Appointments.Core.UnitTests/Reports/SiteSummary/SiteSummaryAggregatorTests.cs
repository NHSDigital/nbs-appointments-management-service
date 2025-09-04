using Nhs.Appointments.Core.Reports.SiteSummary;

namespace Nhs.Appointments.Core.UnitTests.Reports.SiteSummary;

public class SiteSummaryAggregatorTests
{
    private readonly Mock<IBookingAvailabilityStateService> _bookingAvailabilityStateService = new();
    private readonly Mock<IDailySiteSummaryStore> _dailySiteSummaryStore = new();
    private readonly SiteSummaryAggregator _sut;
    private readonly Mock<TimeProvider> _timeProvider = new();

    public SiteSummaryAggregatorTests() => _sut =
        new SiteSummaryAggregator(_bookingAvailabilityStateService.Object, _dailySiteSummaryStore.Object,
            _timeProvider.Object);

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task When_DaysArePassed_IterateCorrectly(int days)
    {
        var site = "site-a";
        var from = new DateOnly(2025, 1, 1);
        var to = from.AddDays(days);

        var daySummary = new DayAvailabilitySummary(new DateOnly(2025, 1, 1), new SessionAvailabilitySummary[]
        {
            new()
            {
                TotalSupportedAppointmentsByService =
                    new Dictionary<string, int> { { "ServiceA", 5 }, { "ServiceB", 5 } },
                Capacity = 10,
                Id = Guid.Empty,
                MaximumCapacity = 20,
            }
        }) { TotalOrphanedAppointmentsByService = { ["ServiceA"] = 0, ["ServiceB"] = 0 } };

        _bookingAvailabilityStateService.Setup(x => x.GetDaySummary(It.IsAny<string>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(new AvailabilitySummary(new[] { daySummary }));

        _dailySiteSummaryStore.Setup(x => x.CreateDailySiteSummary(It.IsAny<DailySiteSummary>()));
        _dailySiteSummaryStore.Setup(x => x.IfExistsDelete(It.IsAny<string>(), It.IsAny<DateOnly>()));
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 1, 1));

        await _sut.AggregateForSite(site, from, to);

        _bookingAvailabilityStateService.Verify(x => x.GetDaySummary(It.IsAny<string>(), It.IsAny<DateOnly>()),
            Times.Exactly(days + 1));
        _dailySiteSummaryStore.Verify(x => x.CreateDailySiteSummary(It.IsAny<DailySiteSummary>()),
            Times.Exactly(days + 1));
        _dailySiteSummaryStore.Verify(x => x.IfExistsDelete(It.IsAny<string>(), It.IsAny<DateOnly>()), Times.Never);
    }

    [Fact]
    public async Task When_NoData_TryToDelete()
    {
        var site = "site-a";
        var from = new DateOnly(2025, 1, 1);
        var to = new DateOnly(2025, 1, 1);

        _bookingAvailabilityStateService.Setup(x => x.GetDaySummary(It.IsAny<string>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(new AvailabilitySummary(new DayAvailabilitySummary[]
            {
                new(new DateOnly(2025, 1, 1), new SessionAvailabilitySummary[]
                {
                    new()
                    {
                        TotalSupportedAppointmentsByService = new Dictionary<string, int>(),
                        Capacity = 0,
                        Id = Guid.Empty,
                        MaximumCapacity = 0,
                    }
                })
            }));
        _dailySiteSummaryStore.Setup(x => x.CreateDailySiteSummary(It.IsAny<DailySiteSummary>()));
        _dailySiteSummaryStore.Setup(x => x.IfExistsDelete(It.IsAny<string>(), It.IsAny<DateOnly>()));
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 1, 1));

        await _sut.AggregateForSite(site, from, to);

        _bookingAvailabilityStateService.Verify(x => x.GetDaySummary(It.IsAny<string>(), It.IsAny<DateOnly>()),
            Times.Once);
        _dailySiteSummaryStore.Verify(x => x.CreateDailySiteSummary(It.IsAny<DailySiteSummary>()), Times.Never);
        _dailySiteSummaryStore.Verify(x => x.IfExistsDelete(It.IsAny<string>(), It.IsAny<DateOnly>()), Times.Once);
    }

    [Fact]
    public async Task When_CancellationsOnly_Save()
    {
        var site = "site-a";
        var from = new DateOnly(2025, 1, 1);
        var to = new DateOnly(2025, 1, 1);

        var daySummary = new DayAvailabilitySummary(new DateOnly(2025, 1, 1), new SessionAvailabilitySummary[]
        {
            new()
            {
                TotalSupportedAppointmentsByService = new Dictionary<string, int>(),
                Capacity = 0,
                Id = Guid.Empty,
                MaximumCapacity = 0,
            }
        }) { TotalCancelledAppointmentsByService = { ["ServiceA"] = 1 } };

        _bookingAvailabilityStateService.Setup(x => x.GetDaySummary(It.IsAny<string>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(new AvailabilitySummary(new[] { daySummary }));
        _dailySiteSummaryStore.Setup(x => x.CreateDailySiteSummary(It.IsAny<DailySiteSummary>()));
        _dailySiteSummaryStore.Setup(x => x.IfExistsDelete(It.IsAny<string>(), It.IsAny<DateOnly>()));
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 1, 1));

        await _sut.AggregateForSite(site, from, to);

        _bookingAvailabilityStateService.Verify(x => x.GetDaySummary(It.IsAny<string>(), It.IsAny<DateOnly>()),
            Times.Once);
        _dailySiteSummaryStore.Verify(x => x.CreateDailySiteSummary(It.IsAny<DailySiteSummary>()), Times.Once);
        _dailySiteSummaryStore.Verify(x => x.IfExistsDelete(It.IsAny<string>(), It.IsAny<DateOnly>()), Times.Never);
    }

    [Fact]
    public async Task When_CapacityOnly_Save()
    {
        var site = "site-a";
        var from = new DateOnly(2025, 1, 1);
        var to = new DateOnly(2025, 1, 1);

        var daySummary = new DayAvailabilitySummary(new DateOnly(2025, 1, 1), new SessionAvailabilitySummary[]
        {
            new()
            {
                TotalSupportedAppointmentsByService = new Dictionary<string, int>(), Capacity = 10, Id = Guid.Empty, MaximumCapacity = 10,
            }
        });

        _bookingAvailabilityStateService.Setup(x => x.GetDaySummary(It.IsAny<string>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(new AvailabilitySummary(new[] { daySummary }));
        _dailySiteSummaryStore.Setup(x => x.CreateDailySiteSummary(It.IsAny<DailySiteSummary>()));
        _dailySiteSummaryStore.Setup(x => x.IfExistsDelete(It.IsAny<string>(), It.IsAny<DateOnly>()));
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 1, 1));

        await _sut.AggregateForSite(site, from, to);

        _bookingAvailabilityStateService.Verify(x => x.GetDaySummary(It.IsAny<string>(), It.IsAny<DateOnly>()),
            Times.Once);
        _dailySiteSummaryStore.Verify(x => x.CreateDailySiteSummary(It.IsAny<DailySiteSummary>()), Times.Once);
        _dailySiteSummaryStore.Verify(x => x.IfExistsDelete(It.IsAny<string>(), It.IsAny<DateOnly>()), Times.Never);
    }

    [Fact]
    public async Task When_OrphanedOnly_Save()
    {
        var site = "site-a";
        var from = new DateOnly(2025, 1, 1);
        var to = new DateOnly(2025, 1, 1);
        
        var daySummary = new DayAvailabilitySummary(new DateOnly(2025, 1, 1), new SessionAvailabilitySummary[]
        {
            new()
            {
                TotalSupportedAppointmentsByService = new Dictionary<string, int>( ),
                Capacity = 10,
                Id = Guid.Empty,
                MaximumCapacity = 10,
            }
        }) { TotalOrphanedAppointmentsByService = { ["ServiceA"] = 1 } };

        _bookingAvailabilityStateService.Setup(x => x.GetDaySummary(It.IsAny<string>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(new AvailabilitySummary(new[] { daySummary }));
        _dailySiteSummaryStore.Setup(x => x.CreateDailySiteSummary(It.IsAny<DailySiteSummary>()));
        _dailySiteSummaryStore.Setup(x => x.IfExistsDelete(It.IsAny<string>(), It.IsAny<DateOnly>()));
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 1, 1));

        await _sut.AggregateForSite(site, from, to);

        _bookingAvailabilityStateService.Verify(x => x.GetDaySummary(It.IsAny<string>(), It.IsAny<DateOnly>()),
            Times.Once);
        _dailySiteSummaryStore.Verify(x => x.CreateDailySiteSummary(It.IsAny<DailySiteSummary>()), Times.Once);
        _dailySiteSummaryStore.Verify(x => x.IfExistsDelete(It.IsAny<string>(), It.IsAny<DateOnly>()), Times.Never);
    }
}
