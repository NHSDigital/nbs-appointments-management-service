namespace Nhs.Appointments.Persistance.BackoffStrategies;

/// <summary>
/// This factory class creates the appropriate backoff strategy based on the container retry configuration.
/// </summary>
internal static class CosmosBackoffStrategyFactory
{
    internal static ICosmosBackoffStrategy Create(ContainerRetryConfiguration containerRetryConfiguration)
    {
        return containerRetryConfiguration.BackoffRetryType switch
        {
            BackoffRetryType.CosmosDefault => new CosmosDefaultBackoffStrategy(containerRetryConfiguration),
            BackoffRetryType.Linear => new CosmosLinearBackoffStrategy(containerRetryConfiguration),
            BackoffRetryType.GeometricDouble => new CosmosGeometricDoubleBackoffStrategy(containerRetryConfiguration),
            BackoffRetryType.Exponential => new CosmosExponentialBackoffStrategy(containerRetryConfiguration),
            _ => throw new ArgumentOutOfRangeException($"BackoffRetryType {containerRetryConfiguration.BackoffRetryType} is not supported by the {nameof(CosmosBackoffStrategyFactory)}")
        };
    }
}
