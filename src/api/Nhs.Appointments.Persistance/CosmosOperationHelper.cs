using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Nhs.Appointments.Persistance;

public static class CosmosOperationHelper
{
    private static int DefaultCosmosMaxRetries => 9;
    
    public static async Task<(T result, double totalRequestCharge)> Retry_CosmosOperation_OnTooManyRequests<T>(
        ContainerRetryConfiguration containerRetryConfiguration,
        Func<Task<T>> cosmosOperation,
        ILogger logger,
        IMetricsRecorder metricsRecorder,
        CancellationToken cancellationToken = default)
    {
        var linkId = Guid.NewGuid();

        var retryRequired = true;
        var cutoffExceeded = false;
        
        var retryCount = 0;

        //make sure default behaviour is a least 10ms
        var customDelayMs = TimeSpan.FromMilliseconds(Math.Max(containerRetryConfiguration.InitialValueMs, 10));
        var customCutoffMs = TimeSpan.FromMilliseconds(containerRetryConfiguration.CutoffRetryMs);

        var totalDelayMs = TimeSpan.FromMilliseconds(0);
        var retryResult = default(T);
        //log metrics for total request charge for the initial attempt, and any retries required
        double totalRequestCharge = 0;

        //used for exponential only
        double exponent = 0;

        //default cosmos can error out before the cutoff reached
        var defaultCosmosTooManyAttempts = false;

        switch (containerRetryConfiguration.BackoffRetryType)
        {
            case BackoffRetryType.CosmosDefault:
            case BackoffRetryType.Linear:
            case BackoffRetryType.GeometricDouble:
                //no work to do
                break;
            case BackoffRetryType.Exponential:
                //derive initial exponent needed to increment next delays, using the provided initial value
                exponent = Math.Log(containerRetryConfiguration.InitialValueMs);

                //increment for next usage
                exponent++;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        while (retryRequired && !cutoffExceeded)
        {
            try
            {
                if (retryCount > 0)
                {
                    logger.LogInformation(
                        "{linkId} - Cosmos TooManyRequests retryCount: {retryCount}, for container: {container}, total delay time ms: {totalDelayMs}",
                        linkId, retryCount, containerRetryConfiguration.ContainerName, totalDelayMs.TotalMilliseconds);
                }

                retryResult = await cosmosOperation();

                if (retryResult is ResponseMessage { StatusCode: HttpStatusCode.TooManyRequests } message)
                {
                    var retryDelay = customDelayMs;
                    
                    //try and use default cosmos retry header response, if CosmosDefault retryType and value exists
                    if (containerRetryConfiguration.BackoffRetryType == BackoffRetryType.CosmosDefault && message.Headers != null && message.Headers.TryGetValue("x-ms-retry-after-ms", out var retryAfterMs))
                    {
                        var milliseconds = long.Parse(retryAfterMs);
                        
                        //force at least 10ms in case cosmos defined retryAfter is too low...
                        retryDelay = TimeSpan.FromMilliseconds(Math.Max(milliseconds, 10));
                    }
                    
                    throw new ResponseMessageException("Database too busy!", statusCode: HttpStatusCode.TooManyRequests, 0, string.Empty, message.Headers?.RequestCharge ?? 0, retryDelay);
                }
                
                //if we get to here, there wasn't a cosmos exception, so no need to retry
                retryRequired = false;
            }
            catch (CosmosException ex)
            {
                totalRequestCharge += ex.RequestCharge;

                if (ex.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    var nextRetryDelayMs = ExtractRetryDelay(ex, containerRetryConfiguration, customDelayMs);
                    
                    //if cosmos and current retry was the last allowed, break out
                    if (containerRetryConfiguration.BackoffRetryType == BackoffRetryType.CosmosDefault && (retryCount) == DefaultCosmosMaxRetries)
                    {
                        defaultCosmosTooManyAttempts = true;
                        break;
                    }
                    
                    //only perform a delay if a retry is required for the next attempt
                    if ((totalDelayMs + nextRetryDelayMs) <= customCutoffMs)
                    {
                        retryCount++;
                        
                        await Task.Delay(nextRetryDelayMs, cancellationToken);
                        
                        //keep a running total delay time for this cosmosOperation
                        totalDelayMs += nextRetryDelayMs;
                        
                        switch (containerRetryConfiguration.BackoffRetryType)
                        {
                            case BackoffRetryType.CosmosDefault:
                            case BackoffRetryType.Linear:
                                //do nothing
                                break;
                            case BackoffRetryType.Exponential:
                                //increment the exponent to derive next delay value needed for exponential backoff
                                customDelayMs = TimeSpan.FromMilliseconds((int)Math.Floor(Math.Exp(exponent++)));
                                break;
                            case BackoffRetryType.GeometricDouble:
                                //double delay time between retries
                                customDelayMs *= 2;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    else
                    {
                        cutoffExceeded = true;
                    }
                }
                else
                {
                    RecordQueryMetrics(metricsRecorder, totalRequestCharge);
                    throw;
                }
            }
        }

        if (defaultCosmosTooManyAttempts)
        {
            var error =
                $"{linkId} - Cosmos TooManyRequests failed after max retries ({DefaultCosmosMaxRetries}) exceeded for container: {containerRetryConfiguration.ContainerName}, total delay time ms: {totalDelayMs.TotalMilliseconds}";
            LogTooManyRequestsError(containerRetryConfiguration, logger, metricsRecorder, linkId, error, totalRequestCharge);
        }
        
        if (cutoffExceeded)
        {
            var error =
                $"{linkId} - Cosmos TooManyRequests failed because the CutoffRetryMs ({containerRetryConfiguration.CutoffRetryMs}) would be exceeded on the next retry attempt : total retries: {retryCount} for container: {containerRetryConfiguration.ContainerName}, total delay time ms: {totalDelayMs.TotalMilliseconds}";
            LogTooManyRequestsError(containerRetryConfiguration, logger, metricsRecorder, linkId, error, totalRequestCharge);
        }

        return (retryResult, totalRequestCharge);
    }

    private static TimeSpan ExtractRetryDelay(CosmosException ex, ContainerRetryConfiguration containerRetryConfiguration, TimeSpan customDelayMs)
    {
        TimeSpan nextRetryDelayMs;
        
        if (ex is ResponseMessageException responseMessageException)
        {
            nextRetryDelayMs = responseMessageException.OverriddenRetryAfter;
        }
        else
        {
            if (containerRetryConfiguration.BackoffRetryType == BackoffRetryType.CosmosDefault &&
                !ex.RetryAfter.HasValue)
            {
                throw new InvalidOperationException("TooManyRequests exception does not have a RetryAfter value");
            }
                    
            nextRetryDelayMs = containerRetryConfiguration.BackoffRetryType == BackoffRetryType.CosmosDefault
                ? ex.RetryAfter!.Value
                : customDelayMs;
        }
        
        return nextRetryDelayMs;
    }
    
    private static void LogTooManyRequestsError(
        ContainerRetryConfiguration containerRetryConfiguration,
        ILogger logger, 
        IMetricsRecorder metricsRecorder, 
        Guid linkId,
        string loggedError, 
        double totalRequestCharge)
    {
        RecordQueryMetrics(metricsRecorder, totalRequestCharge);
        logger.LogError(loggedError);
        throw new InvalidOperationException(
            $"Container '{containerRetryConfiguration.ContainerName}' too many requests were exceeded for linkId: {linkId}");
    }
    
    public static void RecordQueryMetrics(IMetricsRecorder metricsRecorder, double requestCharge)
    {
        metricsRecorder.RecordMetric("RequestCharge", requestCharge);
    }
}
