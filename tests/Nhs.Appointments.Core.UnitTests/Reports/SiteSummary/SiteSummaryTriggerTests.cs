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
        _options.Setup(x => x.Value).Returns(new SiteSummaryOptions { DaysForward = 1, FirstRunDate = new DateOnly(2025, 2, 1) });
        _aggregationStore.Setup(x => x.GetLastRunDate()).ReturnsAsync((DateTimeOffset?)null);
        _aggregationStore.Setup(x => x.SetLastRunDate(It.IsAny<DateTimeOffset>()));
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
        var from = new DateOnly(2025, 2, 1);
        var to = new DateOnly(2025, 7, 2);
        
        await _sut.Trigger();
        
        _timeProvider.Verify(x => x.GetUtcNow(), Times.Once);
        _aggregationStore.Verify(x => x.GetLastRunDate(), Times.Once);
        _aggregationStore.Verify(x => x.SetLastRunDate(It.IsAny<DateTimeOffset>()), Times.Once);
        _messageBus.Verify(x => x.Send(It.Is<AggregateSiteSummaryEvent>(actual => actual.Site == site && actual.From == from && actual.To == to)), Times.Once);
    }
    
    [Fact]
    public async Task When_LastRun_Set()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 7, 1));
        _options.Setup(x => x.Value).Returns(new SiteSummaryOptions { DaysForward = 1, FirstRunDate = new DateOnly(2025, 2, 1)});
        _aggregationStore.Setup(x => x.GetLastRunDate()).ReturnsAsync(new DateTime(2025, 6, 30));
        _aggregationStore.Setup(x => x.SetLastRunDate(It.IsAny<DateTimeOffset>()));
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
        _aggregationStore.Verify(x => x.GetLastRunDate(), Times.Once);
        _aggregationStore.Verify(x => x.SetLastRunDate(It.IsAny<DateTimeOffset>()), Times.Once);
        _messageBus.Verify(x => x.Send(It.Is<AggregateSiteSummaryEvent>(actual => actual.Site == site && actual.From == from && actual.To == to)), Times.Once);
    }
    
    [Fact]
    public async Task When_Multiple_Sites()
    {
        _timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTime(2025, 7, 1));
        _options.Setup(x => x.Value).Returns(new SiteSummaryOptions { DaysForward = 1, FirstRunDate = new DateOnly(2025, 2, 1)});
        _aggregationStore.Setup(x => x.GetLastRunDate()).ReturnsAsync(new DateTime(2025, 6, 30));
        _aggregationStore.Setup(x => x.SetLastRunDate(It.IsAny<DateTimeOffset>()));
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
        _aggregationStore.Verify(x => x.GetLastRunDate(), Times.Once);
        _aggregationStore.Verify(x => x.SetLastRunDate(It.IsAny<DateTimeOffset>()), Times.Once);
        _messageBus.Verify(x => x.Send(It.Is<AggregateSiteSummaryEvent>(actual => actual.Site == site1 && actual.From == from && actual.To == to)), Times.Once);
        _messageBus.Verify(x => x.Send(It.Is<AggregateSiteSummaryEvent>(actual => actual.Site == site2 && actual.From == from && actual.To == to)), Times.Once);
    }
    
    
}
