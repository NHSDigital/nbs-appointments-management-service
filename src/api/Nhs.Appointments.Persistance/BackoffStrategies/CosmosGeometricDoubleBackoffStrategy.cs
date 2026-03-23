using Microsoft.Azure.Cosmos;

namespace Nhs.Appointments.Persistance.BackoffStrategies;

/// <summary>
/// This class implements a geometric double backoff strategy.
/// </summary>
/// <param name="containerRetryConfiguration">The configuration to be used for retrying the database operation.</param>
internal class CosmosGeometricDoubleBackoffStrategy(ContainerRetryConfiguration containerRetryConfiguration) : ICosmosBackoffStrategy
{
    private TimeSpan customDelayMs = TimeSpan.FromMilliseconds(containerRetryConfiguration.InitialValueMs);

    public TimeSpan Backoff(CosmosException ex, CosmosBackoffContext context)
    {
        var nextRetryDelayMs = customDelayMs;
        customDelayMs *= 2;

        return nextRetryDelayMs;
    }
}
