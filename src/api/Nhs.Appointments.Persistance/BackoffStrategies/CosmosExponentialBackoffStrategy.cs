using Microsoft.Azure.Cosmos;

namespace Nhs.Appointments.Persistance.BackoffStrategies;

/// <summary>
/// This class implements an exponential backoff strategy.
/// </summary>
/// <param name="containerRetryConfiguration">The configuration to be used for retrying the database operation.</param>
internal class CosmosExponentialBackoffStrategy(ContainerRetryConfiguration containerRetryConfiguration) : ICosmosBackoffStrategy
{
    private TimeSpan customDelayMs = TimeSpan.FromMilliseconds(containerRetryConfiguration.InitialValueMs);

    private double exponent = Math.Log(containerRetryConfiguration.InitialValueMs) + 1;

    public TimeSpan Backoff(CosmosException ex, CosmosBackoffContext context)
    {
        var nextRetryDelayMs = customDelayMs;
        customDelayMs = TimeSpan.FromMilliseconds((int)Math.Floor(Math.Exp(exponent++)));

        return nextRetryDelayMs;
    }
}
