using CosmosAuditor.AuditSinks;
using CosmosAuditor.Containers;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CosmosAuditor;

public class AuditWorker<TConfig>(
    IEnumerable<IAuditSink> sinks,
    ILogger<AuditWorker<TConfig>> logger,
    TConfig config,
    CosmosClient cosmosClient) : BackgroundService where TConfig : ContainerConfig 
{
    private const string DatabaseName = "appts";
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        var sourceContainerName = config.Name;
        var leaseContainerName = config.LeaseName;

        await CreateLeaseContainerIfDoesntExist();
        var leaseContainer = cosmosClient.GetContainer(DatabaseName, leaseContainerName);
        var changeFeedProcessor = cosmosClient.GetContainer(DatabaseName, sourceContainerName)
            .GetChangeFeedProcessorBuilder<object>(processorName: "changeFeedSample", onChangesDelegate: HandleChangesAsync)
            .WithLeaseAcquireNotification(_onLeaseAcquiredAsync)
            .WithLeaseReleaseNotification(_onLeaseReleaseAsync)
            .WithErrorNotification(_onErrorAsync)
            .WithInstanceName("consoleHost")
            .WithStartTime(DateTime.MinValue.ToUniversalTime())
            .WithLeaseContainer(leaseContainer)
            .Build();

        logger.LogInformation("Starting Change Feed Processor...");
        await changeFeedProcessor.StartAsync();
        logger.LogInformation("Change Feed Processor started.");
    }

    private async Task CreateLeaseContainerIfDoesntExist()
    {
        var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(id: DatabaseName);
        await database.Database.CreateContainerIfNotExistsAsync(id: config.LeaseName, partitionKeyPath: "/id");
    }

    private readonly Container.ChangeFeedMonitorLeaseAcquireDelegate _onLeaseAcquiredAsync = (string leaseToken) =>
    {
        logger.LogInformation($"Lease {leaseToken} is acquired and will start processing");
        return Task.CompletedTask;
    };

    private readonly Container.ChangeFeedMonitorLeaseReleaseDelegate _onLeaseReleaseAsync = (string leaseToken) =>
    {
        logger.LogInformation($"Lease {leaseToken} is released and processing is stopped");
        return Task.CompletedTask;
    };

    private readonly Container.ChangeFeedMonitorErrorDelegate _onErrorAsync = (string leaseToken, Exception exception) =>
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

    private async Task HandleChangesAsync(
        ChangeFeedProcessorContext context,
        IReadOnlyCollection<object> changes,
        CancellationToken cancellationToken)
    {
        logger.LogInformation($"Started handling changes for lease {context.LeaseToken}...");
        logger.LogInformation($"Change Feed request consumed {context.Headers.RequestCharge} RU.");
        // SessionToken if needed to enforce Session consistency on another client instance
        logger.LogInformation($"SessionToken ${context.Headers.Session}");

        // We may want to track any operation's Diagnostics that took longer than some threshold
        if (context.Diagnostics.GetClientElapsedTime() > TimeSpan.FromSeconds(1))
        {
            logger.LogInformation($"Change Feed request took longer than expected. Diagnostics:" + context.Diagnostics.ToString());
        }

        foreach (var item in changes)
        {
            var tasks = sinks.Where(x => config.Sinks.Contains(x.Name)).Select(sink => sink.Consume(config, item));
            
            await Task.WhenAll(tasks);
        }

        logger.LogInformation("Finished handling changes.");
    }
}
