using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Core.Reports.SiteSummary;
using Nhs.Appointments.Jobs.ChangeFeed;

namespace Nhs.Appointments.Jobs.Aggregator;

public class AggregatorSink(ILogger<AggregatorSink> logger, IServiceProvider serviceProvider) : ISink<AggregateSiteSummaryEvent>
{
    public async Task Consume(string source, AggregateSiteSummaryEvent item)
    {
        logger.LogInformation($"Site Summary Aggregation started for {item.Site} {item.Date.ToString("yyyy-MM-dd")}");
        using var scope = serviceProvider.CreateScope();
        
        var cosmosTransaction = scope.ServiceProvider.GetRequiredService<ICosmosTransaction>();
        var siteSummaryAggregator = scope.ServiceProvider.GetRequiredService<ISiteSummaryAggregator>();
        
        await cosmosTransaction.RunJobWithRetry(() => siteSummaryAggregator.AggregateForSite(item.Site, item.Date, item.Date));
    }
}
