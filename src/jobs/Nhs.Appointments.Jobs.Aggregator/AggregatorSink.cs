using Microsoft.Extensions.Logging;
using Nhs.Appointments.Core.Reports.SiteSummary;
using Nhs.Appointments.Jobs.ChangeFeed;

namespace Nhs.Appointments.Jobs.Aggregator;

public class AggregatorSink(ILogger<AggregatorSink> logger, ISiteSummaryAggregator siteSummaryAggregator, ICosmosTransaction cosmosTransaction) : ISink<AggregateSiteSummaryEvent>
{
    public async Task Consume(string source, AggregateSiteSummaryEvent item)
    {
        logger.LogInformation($"Site Summary Aggregation started for {item.Site} {item.Date.ToString("yyyy-MM-dd")}");
        await cosmosTransaction.RunJobWithTry(() => siteSummaryAggregator.AggregateForSite(item.Site, item.Date, item.Date));
    }
}
