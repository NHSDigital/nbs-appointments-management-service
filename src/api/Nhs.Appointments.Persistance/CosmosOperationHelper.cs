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

        var customDelayMs = TimeSpan.FromMilliseconds(containerRetryConfiguration.InitialValueMs);
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

                //if we get to here, there wasn't a cosmos exception, so no need to retry
                retryRequired = false;
            }
            catch (CosmosException ex)
            {
                totalRequestCharge += ex.RequestCharge;

                if (ex.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    if (containerRetryConfiguration.BackoffRetryType == BackoffRetryType.CosmosDefault &&
                        !ex.RetryAfter.HasValue)
                    {
                        throw new InvalidOperationException("TooManyRequests exception does not have a RetryAfter value");
                    }
                    
                    var nextRetryDelayMs = containerRetryConfiguration.BackoffRetryType == BackoffRetryType.CosmosDefault
                        ? ex.RetryAfter!.Value
                        : customDelayMs;
                    
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
