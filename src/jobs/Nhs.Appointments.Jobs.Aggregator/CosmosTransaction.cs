using System.Collections.Concurrent;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Nhs.Appointments.Jobs.Aggregator;

public class CosmosTransaction(ILogger<CosmosTransaction> logger) : ICosmosTransaction
{
    private readonly ConcurrentDictionary<string, bool> _cosmosTransactionLocks = new();

    public async Task RunJobWithTry(string transactionType, Func<Task> action)
    {
        await ResolveTransactionLease(transactionType);
        try
        {
            await action();
        }
        catch (CosmosException ex)
        {
            logger.LogError(ex, "Failed to execute transaction {TransactionType}", transactionType);
        }
        finally
        {
            _cosmosTransactionLocks.Release();
        }
    }

    private async Task SetLock(string transactionType, bool lockValue)
    {
        _cosmosTransactionLocks.AddOrUpdate(transactionType, lockValue, (_, _) => lockValue);
    }

    private async Task ResolveTransactionLock(string transactionType)
    {
        while (true)
        {
            if (_cosmosTransactionLocks.GetOrAdd(transactionType, false))
            {
                logger.LogInformation($"Awaiting lease to execute transaction {transactionType}");
                await Task.Delay(1000);
                continue;
            }

            break;
        }
    }
}
