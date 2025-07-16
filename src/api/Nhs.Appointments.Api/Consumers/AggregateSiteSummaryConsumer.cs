using MassTransit;
using System;
using System.Threading.Tasks;
using Nhs.Appointments.Core.Reports.SiteSummary;

namespace Nhs.Appointments.Api.Consumers;

public class AggregateSiteSummaryConsumer(ISiteSummaryAggregator siteSummaryAggregator) : IConsumer<AggregateSiteSummaryEvent>
{
    public async Task Consume(ConsumeContext<AggregateSiteSummaryEvent> context)
    {
        if (!NotificationIsValid(context.Message))
        {
            throw new InvalidOperationException($"{typeof(AggregateSiteSummaryEvent)} is not valid");
        }

        await siteSummaryAggregator.AggregateForSite(context.Message.Site, context.Message.From , context.Message.To);
    }

    private bool NotificationIsValid(AggregateSiteSummaryEvent e) => e.From <= e.To && !string.IsNullOrWhiteSpace(e.Site);
}
