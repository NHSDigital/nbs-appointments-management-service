using System.Collections.Concurrent;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Nhs.Appointments.Jobs.Aggregator;

public class CosmosTransaction(ILogger<CosmosTransaction> logger) : ICosmosTransaction
{
    private readonly ConcurrentDictionary<string, bool> _cosmosTransactionLocks = new();

    public async Task RunJobWithTry(string transactionType, Func<Task> action)
    {
        var retryCount = 0;
        const int maxRetries = 50;
        await ResolveTransactionLock(transactionType);
        while (retryCount <= maxRetries) {
            try
            {
                await action();
            }
            catch (CosmosException ex)
            {
                SetLock(transactionType, true);
                retryCount++;
                logger.LogError(ex, "Failed to execute transaction {TransactionType} {RetryCount} times", transactionType, retryCount);
                await Task.Delay(ex.RetryAfter ?? TimeSpan.FromSeconds(30));
            }
            finally
            {
                if (retryCount > 0)
                {
                    SetLock(transactionType, false);
                }
            }
        }
        
    }

    private void SetLock(string transactionType, bool lockValue)
    {
        _cosmosTransactionLocks.AddOrUpdate(transactionType, lockValue, (_, _) => lockValue);
    }

    private async Task ResolveTransactionLock(string transactionType)
    {
        while (true)
        {
            if (_cosmosTransactionLocks.GetOrAdd(transactionType, false))
            {
                logger.LogInformation("Awaiting lease to execute transaction {TransactionType}", transactionType);
                await Task.Delay(1000);
                continue;
            }

            break;
        }
    }
}
