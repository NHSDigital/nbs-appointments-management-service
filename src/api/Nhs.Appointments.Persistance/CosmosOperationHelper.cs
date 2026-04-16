using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Nhs.Appointments.Core.Metrics;
using Nhs.Appointments.Persistance.BackoffStrategies;

namespace Nhs.Appointments.Persistance;

public static class CosmosOperationHelper
{
    public static async Task<T> Retry_CosmosOperation_OnTooManyRequests<T>(
        ContainerRetryConfiguration containerRetryConfiguration,
        Func<Task<T>> cosmosOperation,
        ILogger logger,
        IMetricsRecorder metricsRecorder,
        CosmosOperationMetric metric,
        CancellationToken cancellationToken = default)
    {
        var context = new CosmosBackoffContext();

        var backoffStrategy = CosmosBackoffStrategyFactory.Create(containerRetryConfiguration);

        try
        {
            while (true)
            {
                try
                {
                    LogReattempt(containerRetryConfiguration, logger, context);

                    return await Attempt(cosmosOperation, metric, containerRetryConfiguration);
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    metric.AddRuCharge(ex.RequestCharge);
                    var nextRetryDelayMs = backoffStrategy.Backoff(ex, context);

                    if (context.TotalDelayMs + nextRetryDelayMs > containerRetryConfiguration.CutoffRetryTimeSpan)
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

    private static async Task<T> Attempt<T>(Func<Task<T>> cosmosOperation, CosmosOperationMetric metric, ContainerRetryConfiguration containerRetryConfiguration)
    {
        metric.StartAttempt(DateTime.UtcNow);
        try
        {
            var result = await cosmosOperation();
            HandleResponseMessage(result, containerRetryConfiguration);

            return result;
        }
        finally
        {
            metric.EndAttempt(DateTime.UtcNow);
        }
    }

    private static void HandleResponseMessage<T>(T result, ContainerRetryConfiguration containerRetryConfiguration)
    {
        if (result is ResponseMessage message && message.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var retryDelay = containerRetryConfiguration.InitialValueTimeSpan;

            if (message.Headers != null && message.Headers.TryGetValue("x-ms-retry-after-ms", out var retryAfterMs))
            {
                var milliseconds = long.Parse(retryAfterMs);

                // Force at least 10ms in case cosmos defined retryAfterMs is too low.
                retryDelay = TimeSpan.FromMilliseconds(Math.Max(milliseconds, 10));
            }

            throw new ResponseMessageException(message.ErrorMessage, message.StatusCode, 0, message.Headers.ActivityId, message.Headers.RequestCharge, retryDelay);
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
