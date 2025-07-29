using MassTransit;
using Moq;
using Nhs.Appointments.Api.Consumers;
using Nhs.Appointments.Core.Reports.SiteSummary;

namespace Nhs.Appointments.Api.Tests.Consumers;

public class AggregateSiteSummaryConsumerTests
{
    private AggregateSiteSummaryConsumer _sut;
    private readonly Mock<ISiteSummaryAggregator> _siteSummaryAggregator = new();

    public AggregateSiteSummaryConsumerTests()
    {
        _sut = new AggregateSiteSummaryConsumer(_siteSummaryAggregator.Object);
    }
    
    [Theory]
    [InlineData("2025-01-02", "2025-01-01", "site-id")]
    [InlineData("2026-01-01", "2025-01-01", "site-id")]
    [InlineData("2025-05-01", "2025-01-01", "site-id")]
    [InlineData("2025-05-01", "2025-01-02", "")]
    [InlineData("2025-05-01", "2025-01-02", null)]
    [InlineData("2025-05-01", "2025-01-02", "   ")]
    public async Task When_MessageInvalid_ShouldThrow(string from, string to, string site)
    {
        var ctx = new Mock<ConsumeContext<AggregateSiteSummaryEvent>>();
        
        ctx.SetupGet(x => x.Message).Returns(new AggregateSiteSummaryEvent()
        {
            From = DateOnly.ParseExact(from, "yyyy-MM-dd"), To = DateOnly.ParseExact(to, "yyyy-MM-dd"), Site = site
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.Consume(ctx.Object));
    }

    [Theory]
    [InlineData("2025-01-01", "2025-01-01", "site-id")]
    [InlineData("2025-01-01", "2025-01-02", "site-id")]
    [InlineData("2025-01-01", "2025-05-01", "site-id")]
    [InlineData("2025-01-01", "2026-01-01", "site-id")]
    public async Task When_MessageValid_CallsAggregateForSite(string from, string to, string site)
    {
        var message = new AggregateSiteSummaryEvent()
        {
            From = DateOnly.ParseExact(from, "yyyy-MM-dd"), To = DateOnly.ParseExact(to, "yyyy-MM-dd"), Site = site
        };
        
        _siteSummaryAggregator.Setup(x =>
            x.AggregateForSite(It.Is<string>(m => m.Equals(message.Site)), It.Is<DateOnly>(m => m.Equals(message.From)), It.Is<DateOnly>(m => m.Equals(message.To))));
        var ctx = new Mock<ConsumeContext<AggregateSiteSummaryEvent>>();
        
        ctx.SetupGet(x => x.Message).Returns(message);

        await _sut.Consume(ctx.Object);
        
        _siteSummaryAggregator.Verify(x =>
            x.AggregateForSite(It.Is<string>(m => m.Equals(message.Site)), It.Is<DateOnly>(m => m.Equals(message.From)), It.Is<DateOnly>(m => m.Equals(message.To))), Times.Once);
    }
}
