using Microsoft.Azure.Cosmos;

namespace Nhs.Appointments.Persistance.BackoffStrategies;

internal interface ICosmosBackoffStrategy
{
    TimeSpan Backoff(CosmosException ex, CosmosBackoffContext context);
}
