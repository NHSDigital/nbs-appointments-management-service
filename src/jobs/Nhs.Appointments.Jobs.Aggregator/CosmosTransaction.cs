using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Nhs.Appointments.Jobs.Aggregator;

public class CosmosTransaction(ILogger<CosmosTransaction> logger, IOptions<CosmosTransactionOptions> options) : ICosmosTransaction
{
    private readonly CosmosTransactionOptions _options = options.Value;

    public async Task RunJobWithRetry(Func<Task> action, string jobName = "")
    {
        var retryCount = 0;
        while (retryCount <= _options.MaxRetry)
        {
            try
            {
                await action();
                logger.LogInformation("{JobName} Transaction executed successfully after {RetryCount} trys", jobName, retryCount);
                return;
            }
            catch (CosmosException ex)
            {
                retryCount++;
                logger.LogInformation("{JobName} Failed to execute transaction {RetryCount} times, retrying in {RetryTime}", jobName, retryCount, ex.RetryAfter ?? TimeSpan.FromSeconds(_options.DefaultWaitSeconds));
                await Task.Delay(ex.RetryAfter ?? TimeSpan.FromSeconds(_options.DefaultWaitSeconds));
            }
        }
        
        throw new Exception($"{jobName} Failed to execute transaction");
    }
}
