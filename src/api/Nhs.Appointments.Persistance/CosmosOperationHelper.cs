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
        string documentType, // TODO: Revisit this
        // TODO: Add ITimeProvider
        CancellationToken cancellationToken = default)
    {
        var context = new CosmosBackoffContext();

        T retryResult;

        var metric = new CosmosOperationMetric
        {
            Container = containerRetryConfiguration.ContainerName,
            DocumentType = documentType,
        };

        var backoffStrategy = CosmosBackoffStrategyFactory.Create(containerRetryConfiguration);

        try
        {
            while (true)
            {
                try
                {
                    LogReattempt(containerRetryConfiguration, logger, context);

                    retryResult = await Attempt(cosmosOperation, metric);

                    break; // No error if we reach this point.
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    metric.AddRuCharge(ex.RequestCharge); // TODO: Can we always extract the RequestCharge?
                    backoffStrategy.Backoff(ex, context);

                    await Task.Delay(backoffStrategy.NextRetryDelayMs, cancellationToken);
                }
                catch (CosmosException ex)
                {
                    metric.AddRuCharge(ex.RequestCharge);

                    RecordQueryMetrics(metricsRecorder, metric);
                    throw;
                }
            }
        }
        catch (ApplicationException ex) // TODO: Replace with specific exception?
        {
            RecordQueryMetrics(metricsRecorder, metric);
            logger.LogError(ex.Message);

            throw new InvalidOperationException(
                $"Container '{containerRetryConfiguration.ContainerName}' too many requests were exceeded for linkId: {context.LinkId}");
        }

        return (retryResult, metric);
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
            // TODO: Can we add the results request charge here, so as not to need to extract it later? Need to understand the canExtract... boolean.
        }
    }

    private static void LogReattempt(ContainerRetryConfiguration containerRetryConfiguration, ILogger logger, CosmosBackoffContext context)
    {
        if (context.Reattempt)
        {
            logger.LogInformation(
                "{linkId} - Cosmos TooManyRequests retryCount: {retryCount}, for container: {container}, total delay time ms: {totalDelayMs}",
                context.LinkId, context.RetryCount, containerRetryConfiguration.ContainerName, context.TotalDelayMs.TotalMilliseconds);
        }
    }

    public static void RecordQueryMetrics(IMetricsRecorder metricsRecorder, IMetric metric)
    {
        metricsRecorder.RecordMetric(metric.Name, metric);
    }
}
