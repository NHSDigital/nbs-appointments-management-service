using System.Net;
using Microsoft.Azure.Cosmos;

namespace Nhs.Appointments.Persistance;

/// <summary>
/// This wrapper around the native CosmosException allows us to utilise the standard Http 429 too many requests handling process.
/// </summary>
public class ResponseMessageException(
    string message,
    HttpStatusCode statusCode,
    int subStatusCode,
    string activityId,
    double requestCharge,
    TimeSpan fallbackRetryAfter) : CosmosException(message, statusCode, subStatusCode, activityId, requestCharge)
{
    public override TimeSpan? RetryAfter => fallbackRetryAfter;
}
