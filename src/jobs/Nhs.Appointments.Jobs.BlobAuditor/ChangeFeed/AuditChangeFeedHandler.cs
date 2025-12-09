using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Nhs.Appointments.Jobs.BlobAuditor.Configuration;
using Nhs.Appointments.Jobs.BlobAuditor.Sink;

namespace Nhs.Appointments.Jobs.BlobAuditor.ChangeFeed;

public class AuditChangeFeedHandler(
    ILogger<AuditChangeFeedHandler> logger,
    IEnumerable<IBlobSink<JObject>> auditSinks,
    IContainerConfigFactory containerConfigFactory,
    CosmosClient cosmosClient
) : IAuditChangeFeedHandler<JObject>
{
    private const string DatabaseName = "appts";

    public async Task<ChangeFeedProcessor> ResolveChangeFeedForContainer(string containerName)
    {
        var config = containerConfigFactory.CreateContainerConfig(containerName);
        async Task HandleChangesAsync(
            ChangeFeedProcessorContext context, 
            IReadOnlyCollection<JObject> changes,
            CancellationToken cancellationToken
        )
        {
            logger.LogInformation($"Changes detected.");

            foreach (var item in changes)
            {
                var tasks = auditSinks.Select(sink => sink.Consume(containerName, item));

                await Task.WhenAll(tasks);
            }

            logger.LogInformation($"Changes processed.");
        }
    
        var sourceContainerName = config.ContainerName;
        var leaseContainerName = config.LeaseContainerName;
        var processorName = containerName + "_processor";

        await CreateLeaseContainerIfDoesntExist(config);
        var leaseContainer = cosmosClient.GetContainer(DatabaseName, leaseContainerName);
        var changeFeedProcessor = cosmosClient.GetContainer(DatabaseName, sourceContainerName)
            .GetChangeFeedProcessorBuilder<JObject>(processorName: processorName, onChangesDelegate: HandleChangesAsync)
            .WithLeaseAcquireNotification(OnLeaseAcquiredAsync)
            .WithLeaseReleaseNotification(OnLeaseReleaseAsync)
            .WithErrorNotification(OnErrorAsync)
            .WithInstanceName(Environment.MachineName)
            .WithStartTime(DateTime.MinValue.ToUniversalTime())
            .WithLeaseContainer(leaseContainer)
            .Build();
    
        return changeFeedProcessor;
    }

    private async Task CreateLeaseContainerIfDoesntExist(ContainerConfiguration config)
    {
        var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(id: DatabaseName);
        await database.Database.CreateContainerIfNotExistsAsync(id: config.LeaseContainerName, partitionKeyPath: "/id");
    }

    private Container.ChangeFeedMonitorLeaseAcquireDelegate OnLeaseAcquiredAsync => leaseToken =>
    {
        logger.LogInformation($"Lease {leaseToken} is acquired and will start processing");
        return Task.CompletedTask;
    };

    private Container.ChangeFeedMonitorLeaseReleaseDelegate OnLeaseReleaseAsync => leaseToken =>
    {
        logger.LogInformation($"Lease {leaseToken} is released and processing is stopped");
        return Task.CompletedTask;
    };

    private Container.ChangeFeedMonitorErrorDelegate OnErrorAsync => (leaseToken, exception) =>
    {
        if (exception is ChangeFeedProcessorUserException userException)
        {
            logger.LogError($"Lease {leaseToken} processing failed with unhandled exception from user delegate {userException.InnerException}");
        }
        else
        {
            logger.LogError($"Lease {leaseToken} failed with {exception}");
        }

        return Task.CompletedTask;
    };
}
