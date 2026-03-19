using Microsoft.Azure.Cosmos;

namespace Nhs.Appointments.Persistance.BackoffStrategies;

internal interface ICosmosBackoffStrategy
{
    void Backoff(CosmosException ex, CosmosBackoffContext context);

    TimeSpan NextRetryDelayMs { get; }
}
