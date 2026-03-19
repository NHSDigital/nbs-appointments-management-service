using Microsoft.Azure.Cosmos;

namespace Nhs.Appointments.Persistance.BackoffStrategies;

/// <summary>
/// This class implements a geometric double backoff strategy.
/// </summary>
/// <param name="containerRetryConfiguration">The configuration to be used for retrying the database operation.</param>
internal class CosmosGeometricDoubleBackoffStrategy(ContainerRetryConfiguration containerRetryConfiguration) 
    : BaseCosmosBackoffStrategy(containerRetryConfiguration)
{
    private TimeSpan customDelayMs = TimeSpan.FromMilliseconds(containerRetryConfiguration.InitialValueMs);

    public override void Backoff(CosmosException ex, CosmosBackoffContext context)
    {
        NextRetryDelayMs = customDelayMs;
        customDelayMs *= 2;

        base.Backoff(ex, context);
    }
}
