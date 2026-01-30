using System.Collections.Concurrent;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Nhs.Appointments.Jobs.Aggregator;

public class CosmosTransaction(ILogger<CosmosTransaction> logger, IOptions<CosmosTransactionOptions> options) : ICosmosTransaction
{
    private readonly CosmosTransactionOptions _options = options.Value;

    public async Task RunJobWithRetry(Func<Task> action)
    {
        var retryCount = 0;
        while (retryCount <= _options.MaxRetry)
        {
            try
            {
                await action();
                logger.LogInformation("Transaction executed successfully after {RetryCount} trys", retryCount);
                return;
            }
            catch (CosmosException ex)
            {
                retryCount++;
                logger.LogInformation("Failed to execute transaction {RetryCount} times, retrying in {RetryTime}", retryCount, ex.RetryAfter ?? TimeSpan.FromSeconds(_options.DefaultWaitSeconds));
                await Task.Delay(ex.RetryAfter ?? TimeSpan.FromSeconds(_options.DefaultWaitSeconds));
            }
        }
        
        throw new Exception("Failed to execute transaction");
    }
}
