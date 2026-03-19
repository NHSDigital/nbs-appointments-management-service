namespace Nhs.Appointments.Persistance.BackoffStrategies;

/// <summary>
/// This class implements a linear backoff strategy.
/// </summary>
/// <param name="containerRetryConfiguration">The configuration to be used for retrying the database operation.</param>
internal class CosmosLinearBackoffStrategy : BaseCosmosBackoffStrategy
{
    public CosmosLinearBackoffStrategy(ContainerRetryConfiguration containerRetryConfiguration) : base(containerRetryConfiguration)
    {
        NextRetryDelayMs = TimeSpan.FromMilliseconds(ContainerRetryConfiguration.InitialValueMs);
    }
}
