using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Core.Metrics;
using Nhs.Appointments.Persistance.BackoffStrategies;

namespace Nhs.Appointments.Persistance;

public static class CosmosOperationHelper
{
    public static async Task<(T result, CosmosOperationMetric resultantMetric)> Retry_CosmosOperation_OnTooManyRequests<T>(
        ContainerRetryConfiguration containerRetryConfiguration,
        Func<Task<T>> cosmosOperation,
        ILogger logger,
        IMetricsRecorder metricsRecorder,
        string documentType,
        string path,
        // TODO: Add ITimeProvider
        CancellationToken cancellationToken = default)
    {
        var context = new CosmosBackoffContext();
        var customCutoffMs = TimeSpan.FromMilliseconds(containerRetryConfiguration.CutoffRetryMs);

        var metric = new CosmosOperationMetric
        {
            Container = containerRetryConfiguration.ContainerName,
            DocumentType = documentType,
            Path = path
        };

        var backoffStrategy = CosmosBackoffStrategyFactory.Create(containerRetryConfiguration);

        try
        {
            while (true)
            {
                try
                {
                    LogReattempt(containerRetryConfiguration, logger, context);

                    var retryResult = await Attempt(cosmosOperation, metric);

                    // TODO: Can we safely add the results request charge by extracting it here, so as not to need to extract it in the caller? Need to understand the "canExtractRequestCharge" boolean.
                    return (retryResult, metric); // No error if we reach this point.
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    metric.AddRuCharge(ex.RequestCharge);
                    var nextRetryDelayMs = backoffStrategy.Backoff(ex, context);

                    if (context.TotalDelayMs + nextRetryDelayMs > customCutoffMs)
                    {
                        var error =
                            $"{context.LinkId} - Cosmos TooManyRequests failed because the CutoffRetryMs ({containerRetryConfiguration.CutoffRetryMs}) would be exceeded on the next retry attempt : total retries: {context.RetryCount} for container: {containerRetryConfiguration.ContainerName}, total delay time ms: {context.TotalDelayMs.TotalMilliseconds}";

                        throw new BackoffException(error);
                    }

                    context.RecordBackoff(nextRetryDelayMs);

                    await Task.Delay(nextRetryDelayMs, cancellationToken);
                }
                catch (CosmosException ex)
                {
                    metric.AddRuCharge(ex.RequestCharge);

                    RecordQueryMetrics(metricsRecorder, metric);
                    throw;
                }
            }
        }
        catch (BackoffException ex)
        {
            RecordQueryMetrics(metricsRecorder, metric);
            logger.LogError(ex, "A backoff exception was thrown.");

            throw new InvalidOperationException(
                $"Container '{containerRetryConfiguration.ContainerName}' too many requests were exceeded for linkId: {context.LinkId}");
        }
    }

    private static async Task<T> Attempt<T>(Func<Task<T>> cosmosOperation, CosmosOperationMetric metric)
    {
        metric.StartAttempt(DateTime.UtcNow);
        try
        {
            return await cosmosOperation();
        }
        finally
        {
            metric.EndAttempt(DateTime.UtcNow);
        }
    }

    private static void LogReattempt(ContainerRetryConfiguration containerRetryConfiguration, ILogger logger, CosmosBackoffContext context)
    {
        if (context.IsReattempt)
        {
            logger.LogInformation(
                "{linkId} - Cosmos TooManyRequests retryCount: {retryCount}, for container: {container}, total delay time ms: {totalDelayMs}",
                context.LinkId, context.RetryCount, containerRetryConfiguration.ContainerName, context.TotalDelayMs.TotalMilliseconds);
        }
    }

    public static void RecordQueryMetrics(IMetricsRecorder metricsRecorder, IMetric metric)
    {
        metricsRecorder.RecordMetric(metric);
    }
}
