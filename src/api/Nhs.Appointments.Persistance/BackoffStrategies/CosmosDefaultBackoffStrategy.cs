using Microsoft.Azure.Cosmos;

namespace Nhs.Appointments.Persistance.BackoffStrategies;

/// <summary>
/// This class implements the Cosmos default backoff strategy.
/// </summary>
/// <param name="containerRetryConfiguration">The configuration to be used for retrying the database operation.</param>
internal class CosmosDefaultBackoffStrategy(ContainerRetryConfiguration containerRetryConfiguration) : ICosmosBackoffStrategy
{
    public const int DefaultCosmosMaxRetries = 9;

    public TimeSpan Backoff(CosmosException ex, CosmosBackoffContext context)
    {
        EnsureMaxRetriesNotReached(context);

        return GetBackoffValue(ex);
    }

    private TimeSpan GetBackoffValue(CosmosException ex)
    {
        if (ex is ResponseMessageException)
        {
            return containerRetryConfiguration.InitialValueTimeSpan;
        }

        if (!ex.RetryAfter.HasValue)
        {
            // TODO: Should the metrics be being logged in this situation? If so, we need to track this in the caller.
            throw new InvalidOperationException("TooManyRequests exception does not have a RetryAfter value");
        }

        return ex.RetryAfter.Value;
    }

    private void EnsureMaxRetriesNotReached(CosmosBackoffContext context)
    {
        if (context.RetryCount == DefaultCosmosMaxRetries)
        {
            var error =
                $"{context.LinkId} - Cosmos TooManyRequests failed after max retries ({DefaultCosmosMaxRetries}) exceeded for container: {containerRetryConfiguration.ContainerName}, total delay time ms: {context.TotalDelayMs.TotalMilliseconds}";
            throw new BackoffException(error);
        }
    }
}
