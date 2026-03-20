using Microsoft.Azure.Cosmos;

namespace Nhs.Appointments.Persistance.BackoffStrategies;

/// <summary>
/// This class implements a linear backoff strategy.
/// </summary>
/// <param name="containerRetryConfiguration">The configuration to be used for retrying the database operation.</param>
internal class CosmosLinearBackoffStrategy(ContainerRetryConfiguration containerRetryConfiguration) : ICosmosBackoffStrategy
{
    public TimeSpan NextRetryDelayMs { get; } = TimeSpan.FromMilliseconds(containerRetryConfiguration.InitialValueMs);

    public void Backoff(CosmosException ex, CosmosBackoffContext context) { }
}
