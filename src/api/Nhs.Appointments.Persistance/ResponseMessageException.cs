using System.Net;
using Microsoft.Azure.Cosmos;

namespace Nhs.Appointments.Persistance;

public class ResponseMessageException(
    string message,
    HttpStatusCode statusCode,
    int subStatusCode,
    string activityId,
    double requestCharge,
    TimeSpan overriddenRetryAfter)
    : CosmosException(message, statusCode, subStatusCode, activityId, requestCharge)

{
    public TimeSpan OverriddenRetryAfter { get; set; } = overriddenRetryAfter;
}
