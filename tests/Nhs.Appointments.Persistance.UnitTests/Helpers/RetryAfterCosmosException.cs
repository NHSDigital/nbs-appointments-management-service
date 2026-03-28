using System.Net;
using Microsoft.Azure.Cosmos;

namespace Nhs.Appointments.Persistance.UnitTests.Helpers;

internal sealed class RetryAfterCosmosException(TimeSpan retryAfter) : CosmosException("Boom",
    HttpStatusCode.TooManyRequests, subStatusCode: 0,
    activityId: Guid.NewGuid().ToString(), requestCharge: 2)
{
    public override TimeSpan? RetryAfter => retryAfter;
}
