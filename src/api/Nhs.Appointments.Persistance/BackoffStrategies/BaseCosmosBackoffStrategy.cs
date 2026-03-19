using Microsoft.Azure.Cosmos;

namespace Nhs.Appointments.Persistance.BackoffStrategies;

internal abstract class BaseCosmosBackoffStrategy : ICosmosBackoffStrategy
{
    private readonly TimeSpan customCutoffMs;

    protected ContainerRetryConfiguration ContainerRetryConfiguration { get; }

    public BaseCosmosBackoffStrategy(ContainerRetryConfiguration containerRetryConfiguration)
    {
        ContainerRetryConfiguration = containerRetryConfiguration;
        customCutoffMs = TimeSpan.FromMilliseconds(containerRetryConfiguration.CutoffRetryMs);
    }

    public TimeSpan NextRetryDelayMs { get; protected set; } // Descendant classes set this according to their own specific stance.

    public virtual void Backoff(CosmosException ex, CosmosBackoffContext context) 
    {
        if (context.TotalDelayMs + NextRetryDelayMs > customCutoffMs)
        {
            var error =
                $"{context.LinkId} - Cosmos TooManyRequests failed because the CutoffRetryMs ({ContainerRetryConfiguration.CutoffRetryMs}) would be exceeded on the next retry attempt : total retries: {context.RetryCount} for container: {ContainerRetryConfiguration.ContainerName}, total delay time ms: {context.TotalDelayMs.TotalMilliseconds}";

            throw new ApplicationException(error);
        }

        context.RecordBackoff(NextRetryDelayMs);
    }
}
