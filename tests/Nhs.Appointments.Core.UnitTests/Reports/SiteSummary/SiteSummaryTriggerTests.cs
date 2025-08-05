using Microsoft.Extensions.Options;
using Nhs.Appointments.Core.Messaging;
using Nhs.Appointments.Core.Reports;
using Nhs.Appointments.Core.Reports.SiteSummary;

namespace Nhs.Appointments.Core.UnitTests.Reports.SiteSummary;

public class SiteSummaryTriggerTests
{
    private readonly SiteSummaryTrigger _sut;
    private readonly Mock<IOptions<SiteSummaryOptions>> _options = new();
    private readonly Mock<TimeProvider> _timeProvider = new();
    private readonly Mock<IAggregationStore> _aggregationStore = new();
    private readonly Mock<ISiteService> _siteService = new();
    private readonly Mock<IMessageBus> _messageBus = new();
    
    public SiteSummaryTriggerTests() => _sut =
        new SiteSummaryTrigger(_options.Object, _timeProvider.Object, _aggregationStore.Object, _siteService.Object, _messageBus.Object);

    [Fact]
    public async Task When_LastRun_Null()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 7, 1));
        _options.Setup(x => x.Value).Returns(new SiteSummaryOptions { DaysForward = 1, DaysChunkSize = 2, FirstRunDate = new DateOnly(2025, 2, 1) });
        _aggregationStore.Setup(x => x.GetLastRun()).ReturnsAsync((Aggregation)null);
        _aggregationStore.Setup(x => x.SetLastRun(It.IsAny<DateTimeOffset>(), It.IsAny<DateOnly>(),It.IsAny<DateOnly>(),It.IsAny<DateOnly>()));
        _siteService.Setup(x => x.GetAllSites()).ReturnsAsync(new List<Site>
        {
            new (
                "site-1",
                "site-name-1",
                "ADDR",
                "PHONE",
                "ODS",
                "REGION",
                "ICB",
                "INFO",
                new Accessibility[] { },
                new Location("test", [0.0]))
        });
        
        await _sut.Trigger();
        
        _timeProvider.Verify(x => x.GetUtcNow(), Times.Once);
        _aggregationStore.Verify(x => x.GetLastRun(), Times.Once);
        _aggregationStore.Verify(x => x.SetLastRun(It.IsAny<DateTimeOffset>(), It.IsAny<DateOnly>(),It.IsAny<DateOnly>(),It.IsAny<DateOnly>()), Times.Once);
    }
    
    [Fact]
    public async Task When_LastRun_Finished_DayOld()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 7, 1));
        _options.Setup(x => x.Value).Returns(new SiteSummaryOptions { DaysForward = 1, DaysChunkSize = 2, FirstRunDate = new DateOnly(2025, 2, 1)});
        _aggregationStore.Setup(x => x.GetLastRun()).ReturnsAsync(new Aggregation()
        {
            LastTriggeredUtcDate = new DateTime(2025, 6, 30),
            FromDateOnly = new DateOnly(2025,1,1),
            ToDateOnly = new DateOnly(2025, 6, 30),
            LastRanToDateOnly = new DateOnly(2025, 6, 30)
        } );
        _aggregationStore.Setup(x => x.SetLastRun(It.IsAny<DateTimeOffset>(), It.IsAny<DateOnly>(),It.IsAny<DateOnly>(),It.IsAny<DateOnly>()));
        _siteService.Setup(x => x.GetAllSites()).ReturnsAsync(new List<Site>
        {
            new (
                "site-1",
                "site-name-1",
                "ADDR",
                "PHONE",
                "ODS",
                "REGION",
                "ICB",
                "INFO",
                new Accessibility[] { },
                new Location("test", [0.0]))
        });

        var site = "site-1";
        var from = new DateOnly(2025, 6, 30);
        var to = new DateOnly(2025, 7, 2);
        
        await _sut.Trigger();
        
        _timeProvider.Verify(x => x.GetUtcNow(), Times.Once);
        _aggregationStore.Verify(x => x.GetLastRun(), Times.Once);
        _aggregationStore.Verify(x => x.SetLastRun(It.IsAny<DateTimeOffset>(), It.IsAny<DateOnly>(),It.IsAny<DateOnly>(),It.IsAny<DateOnly>()), Times.Once);
        _messageBus.Verify(x => x.Send(It.Is<AggregateSiteSummaryEvent[]>(actual => actual[0].Site == site && actual[0].From == from && actual[0].To == to)), Times.Once);
    }
    
    [Fact]
    public async Task When_RunningFullDay_Iterates_AsExpected()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 6, 30));
        _options.Setup(x => x.Value).Returns(new SiteSummaryOptions { DaysForward = 10, DaysChunkSize = 2, FirstRunDate = new DateOnly(2025, 2, 1)});
        _aggregationStore.Setup(x => x.GetLastRun()).ReturnsAsync(new Aggregation()
        {
            LastTriggeredUtcDate = new DateTime(2025, 6, 29),
            FromDateOnly = new DateOnly(2025,1,1),
            ToDateOnly = new DateOnly(2025, 7, 9),
            LastRanToDateOnly = new DateOnly(2025, 7, 9)
        } );
        _aggregationStore.Setup(x => x.SetLastRun(It.IsAny<DateTimeOffset>(), It.IsAny<DateOnly>(),It.IsAny<DateOnly>(),It.IsAny<DateOnly>()));
        _siteService.Setup(x => x.GetAllSites()).ReturnsAsync(new List<Site>
        {
            new (
                "site-1",
                "site-name-1",
                "ADDR",
                "PHONE",
                "ODS",
                "REGION",
                "ICB",
                "INFO",
                new Accessibility[] { },
                new Location("test", [0.0]))
        });

        var site = "site-1";
        var from = new DateOnly(2025, 6, 29);
        var to = new DateOnly(2025, 7, 1);
        
        await _sut.Trigger();
        
        _timeProvider.Verify(x => x.GetUtcNow(), Times.Once);
        _aggregationStore.Verify(x => x.GetLastRun(), Times.Once);
        _aggregationStore.Verify(x => x.SetLastRun(It.IsAny<DateTimeOffset>(), new DateOnly(2025, 6, 29),new DateOnly(2025, 7, 10),new DateOnly(2025, 7, 1)), Times.Once);
        _messageBus.Verify(x => x.Send(It.Is<AggregateSiteSummaryEvent[]>(actual => actual[0].Site == site && actual[0].From == from && actual[0].To == to)), Times.Once);
        
        _aggregationStore.Setup(x => x.GetLastRun()).ReturnsAsync(new Aggregation()
        {
            LastTriggeredUtcDate = new DateTime(2025, 6, 30),
            FromDateOnly = new DateOnly(2025,6,29),
            ToDateOnly = new DateOnly(2025, 7, 10),
            LastRanToDateOnly = new DateOnly(2025, 7, 1)
        });
        
        from = new DateOnly(2025, 7, 2);
        to = new DateOnly(2025, 7, 4);
        
        await _sut.Trigger();
        
        _timeProvider.Verify(x => x.GetUtcNow(), Times.Exactly(2));
        _aggregationStore.Verify(x => x.GetLastRun(), Times.Exactly(2));
        _aggregationStore.Verify(x => x.SetLastRun(It.IsAny<DateTimeOffset>(), new DateOnly(2025, 6, 29),new DateOnly(2025, 7, 10),new DateOnly(2025, 7, 4)), Times.Once);
        _messageBus.Verify(x => x.Send(It.Is<AggregateSiteSummaryEvent[]>(actual => actual[0].Site == site && actual[0].From == from && actual[0].To == to)), Times.Once);
        
        _aggregationStore.Setup(x => x.GetLastRun()).ReturnsAsync(new Aggregation()
        {
            LastTriggeredUtcDate = new DateTime(2025, 6, 30),
            FromDateOnly = new DateOnly(2025,6,29),
            ToDateOnly = new DateOnly(2025, 7, 10),
            LastRanToDateOnly = new DateOnly(2025, 7, 4)
        });
        
        from = new DateOnly(2025, 7, 5);
        to = new DateOnly(2025, 7, 7);
        
        await _sut.Trigger();
        
        _timeProvider.Verify(x => x.GetUtcNow(), Times.Exactly(3));
        _aggregationStore.Verify(x => x.GetLastRun(), Times.Exactly(3));
        _aggregationStore.Verify(x => x.SetLastRun(It.IsAny<DateTimeOffset>(), new DateOnly(2025, 6, 29),new DateOnly(2025, 7, 10),new DateOnly(2025, 7, 7)), Times.Once);
        _messageBus.Verify(x => x.Send(It.Is<AggregateSiteSummaryEvent[]>(actual => actual[0].Site == site && actual[0].From == from && actual[0].To == to)), Times.Once);
        
        _aggregationStore.Setup(x => x.GetLastRun()).ReturnsAsync(new Aggregation()
        {
            LastTriggeredUtcDate = new DateTime(2025, 6, 30),
            FromDateOnly = new DateOnly(2025,6,29),
            ToDateOnly = new DateOnly(2025, 7, 10),
            LastRanToDateOnly = new DateOnly(2025, 7, 7)
        });
        
        from = new DateOnly(2025, 7, 8);
        to = new DateOnly(2025, 7, 10);
        
        await _sut.Trigger();
        
        _timeProvider.Verify(x => x.GetUtcNow(), Times.Exactly(4));
        _aggregationStore.Verify(x => x.GetLastRun(), Times.Exactly(4));
        _aggregationStore.Verify(x => x.SetLastRun(It.IsAny<DateTimeOffset>(), new DateOnly(2025, 6, 29),new DateOnly(2025, 7, 10),new DateOnly(2025, 7, 10)), Times.Once);
        _messageBus.Verify(x => x.Send(It.Is<AggregateSiteSummaryEvent[]>(actual => actual[0].Site == site && actual[0].From == from && actual[0].To == to)), Times.Once);
        
        _aggregationStore.Setup(x => x.GetLastRun()).ReturnsAsync(new Aggregation()
        {
            LastTriggeredUtcDate = new DateTime(2025, 6, 30),
            FromDateOnly = new DateOnly(2025,6,29),
            ToDateOnly = new DateOnly(2025, 7, 10),
            LastRanToDateOnly = new DateOnly(2025, 7, 10)
        });
        
        from = new DateOnly(2025, 7, 9);
        to = new DateOnly(2025, 7, 10);
        
        await _sut.Trigger();
        
        _timeProvider.Verify(x => x.GetUtcNow(), Times.Exactly(5));
        _aggregationStore.Verify(x => x.GetLastRun(), Times.Exactly(5));
        // Should not trigger again. Above verify will hit 4 times, this run should not hit them again
        _aggregationStore.Verify(x => x.SetLastRun(It.IsAny<DateTimeOffset>(), It.IsAny<DateOnly>(),It.IsAny<DateOnly>(),It.IsAny<DateOnly>()), Times.Exactly(4));
        _messageBus.Verify(x => x.Send(It.IsAny<AggregateSiteSummaryEvent[]>()), Times.Exactly(4));
    }
    
    [Fact]
    public async Task When_LastRun_NotFinished_DayOld()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 7, 1));
        _options.Setup(x => x.Value).Returns(new SiteSummaryOptions { DaysForward = 1, DaysChunkSize = 2, FirstRunDate = new DateOnly(2025, 2, 1)});
        _aggregationStore.Setup(x => x.GetLastRun()).ReturnsAsync(new Aggregation()
        {
            LastTriggeredUtcDate = new DateTime(2025, 6, 30),
            FromDateOnly = new DateOnly(2025,1,1),
            ToDateOnly = new DateOnly(2025, 6, 30),
            LastRanToDateOnly = new DateOnly(2025, 6, 28)
        } );
        _aggregationStore.Setup(x => x.SetLastRun(It.IsAny<DateTimeOffset>(), It.IsAny<DateOnly>(),It.IsAny<DateOnly>(),It.IsAny<DateOnly>()));
        _siteService.Setup(x => x.GetAllSites()).ReturnsAsync(new List<Site>
        {
            new (
                "site-1",
                "site-name-1",
                "ADDR",
                "PHONE",
                "ODS",
                "REGION",
                "ICB",
                "INFO",
                new Accessibility[] { },
                new Location("test", [0.0]))
        });

        var site = "site-1";
        var from = new DateOnly(2025, 6, 29);
        var to = new DateOnly(2025, 6, 30);
        
        await _sut.Trigger();
        
        _timeProvider.Verify(x => x.GetUtcNow(), Times.Once);
        _aggregationStore.Verify(x => x.GetLastRun(), Times.Once);
        _aggregationStore.Verify(x => x.SetLastRun(It.IsAny<DateTimeOffset>(), It.IsAny<DateOnly>(),It.IsAny<DateOnly>(),It.IsAny<DateOnly>()), Times.Once);
        _messageBus.Verify(x => x.Send(It.Is<AggregateSiteSummaryEvent[]>(actual => actual[0].Site == site && actual[0].From == from && actual[0].To == to)), Times.Once);
    }
    
    [Fact]
    public async Task When_LastRun_NotFinished_SameDay()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 6, 30));
        _options.Setup(x => x.Value).Returns(new SiteSummaryOptions { DaysForward = 1, DaysChunkSize = 2, FirstRunDate = new DateOnly(2025, 2, 1)});
        _aggregationStore.Setup(x => x.GetLastRun()).ReturnsAsync(new Aggregation()
        {
            LastTriggeredUtcDate = new DateTime(2025, 6, 30),
            FromDateOnly = new DateOnly(2025,1,1),
            ToDateOnly = new DateOnly(2025, 6, 30),
            LastRanToDateOnly = new DateOnly(2025, 6, 28)
        } );
        _aggregationStore.Setup(x => x.SetLastRun(It.IsAny<DateTimeOffset>(), It.IsAny<DateOnly>(),It.IsAny<DateOnly>(),It.IsAny<DateOnly>()));
        _siteService.Setup(x => x.GetAllSites()).ReturnsAsync(new List<Site>
        {
            new (
                "site-1",
                "site-name-1",
                "ADDR",
                "PHONE",
                "ODS",
                "REGION",
                "ICB",
                "INFO",
                new Accessibility[] { },
                new Location("test", [0.0]))
        });

        var site = "site-1";
        var from = new DateOnly(2025, 6, 29);
        var to = new DateOnly(2025, 6, 30);
        
        await _sut.Trigger();
        
        _timeProvider.Verify(x => x.GetUtcNow(), Times.Once);
        _aggregationStore.Verify(x => x.GetLastRun(), Times.Once);
        _aggregationStore.Verify(x => x.SetLastRun(It.IsAny<DateTimeOffset>(), It.IsAny<DateOnly>(),It.IsAny<DateOnly>(),It.IsAny<DateOnly>()), Times.Once);
        _messageBus.Verify(x => x.Send(It.Is<AggregateSiteSummaryEvent[]>(actual => actual[0].Site == site && actual[0].From == from && actual[0].To == to)), Times.Once);
    }
    
    [Fact]
    public async Task When_LastRun_Finished_SameDay()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 6, 30));
        _options.Setup(x => x.Value).Returns(new SiteSummaryOptions { DaysForward = 1, DaysChunkSize = 2, FirstRunDate = new DateOnly(2025, 2, 1)});
        _aggregationStore.Setup(x => x.GetLastRun()).ReturnsAsync(new Aggregation()
        {
            LastTriggeredUtcDate = new DateTime(2025, 6, 30),
            FromDateOnly = new DateOnly(2025,1,1),
            ToDateOnly = new DateOnly(2025, 6, 30),
            LastRanToDateOnly = new DateOnly(2025, 6, 30)
        } );
        
        await _sut.Trigger();
        
        _timeProvider.Verify(x => x.GetUtcNow(), Times.Once);
        _aggregationStore.Verify(x => x.GetLastRun(), Times.Once);
        _aggregationStore.Verify(x => x.SetLastRun(It.IsAny<DateTimeOffset>(), It.IsAny<DateOnly>(),It.IsAny<DateOnly>(),It.IsAny<DateOnly>()), Times.Never);
        _messageBus.Verify(x => x.Send(It.IsAny<AggregateSiteSummaryEvent[]>()), Times.Never());
    }
    
    [Fact]
    public async Task When_Multiple_Sites()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 7, 1));
        _options.Setup(x => x.Value).Returns(new SiteSummaryOptions { DaysForward = 1, DaysChunkSize = 2, FirstRunDate = new DateOnly(2025, 2, 1)});
        _aggregationStore.Setup(x => x.GetLastRun()).ReturnsAsync(new  Aggregation
        {
            LastTriggeredUtcDate = new DateTime(2025, 6, 30),
            FromDateOnly = new DateOnly(2025,1,1),
            ToDateOnly = new DateOnly(2025, 7, 1),
            LastRanToDateOnly = new DateOnly(2025, 7, 1)
        });
        _aggregationStore.Setup(x => x.SetLastRun(It.IsAny<DateTimeOffset>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>()));
        _siteService.Setup(x => x.GetAllSites()).ReturnsAsync(new List<Site>
        {
            new (
                "site-1",
                "site-name-1",
                "ADDR",
                "PHONE",
                "ODS",
                "REGION",
                "ICB",
                "INFO",
                new Accessibility[] { },
                new Location("test", [0.0])),
            new (
                "site-2",
                "site-name-2",
                "ADDR",
                "PHONE",
                "ODS",
                "REGION",
                "ICB",
                "INFO",
                new Accessibility[] { },
                new Location("test", [0.0]))
        });

        var site1 = "site-1";
        var site2 = "site-1";
        var from = new DateOnly(2025, 6, 30);
        var to = new DateOnly(2025, 7, 2);
        
        await _sut.Trigger();
        
        _timeProvider.Verify(x => x.GetUtcNow(), Times.Once);
        _aggregationStore.Verify(x => x.GetLastRun(), Times.Once);
        _aggregationStore.Verify(x => x.SetLastRun(It.IsAny<DateTimeOffset>(), It.IsAny<DateOnly>(),It.IsAny<DateOnly>(),It.IsAny<DateOnly>()), Times.Once);
        _messageBus.Verify(x => x.Send(It.Is<AggregateSiteSummaryEvent[]>(actual => actual[0].Site == site1 && actual[0].From == from && actual[0].To == to)), Times.Once);
        _messageBus.Verify(x => x.Send(It.Is<AggregateSiteSummaryEvent[]>(actual => actual[0].Site == site2 && actual[0].From == from && actual[0].To == to)), Times.Once);
    }
    
    
}
