using MassTransit;
using System;
using System.Threading.Tasks;
using Nhs.Appointments.Core.Reports.SiteSummary;

namespace Nhs.Appointments.Api.Consumers;

public abstract class AggregateSiteSummaryConsumer(ISiteSummaryAggregator siteSummaryAggregator) : IConsumer<AggregateSiteSummaryEvent>
{
    public async Task Consume(ConsumeContext<AggregateSiteSummaryEvent> context)
    {
        if (!NotificationIsValid(context.Message))
        {
            throw new InvalidOperationException($"{typeof(AggregateSiteSummaryEvent)} is not valid");
        }

        var from = context.Message.From ?? new DateOnly(2025, 02, 1);
        var to = context.Message.To;

        await siteSummaryAggregator.AggregateForSite(context.Message.Site, from, to);
    }

    private bool NotificationIsValid(AggregateSiteSummaryEvent e) => (e.From is null || e.From <= e.To) && !string.IsNullOrWhiteSpace(e.Site);
}
