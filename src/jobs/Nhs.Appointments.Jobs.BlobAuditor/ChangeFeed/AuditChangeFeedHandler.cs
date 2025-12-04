using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Nhs.Appointments.Jobs.BlobAuditor.Sink;

namespace Nhs.Appointments.Jobs.BlobAuditor.ChangeFeed;

public class AuditChangeFeedHandler(ILogger<AuditChangeFeedHandler> logger, IEnumerable<ISink<JObject>> auditSinks) : IChangeFeedHandler<JObject>
{
    public 
    public Container.ChangeFeedMonitorLeaseAcquireDelegate OnLeaseAcquiredAsync => leaseToken =>
    {
        logger.LogInformation($"Lease {leaseToken} is acquired and will start processing");
        return Task.CompletedTask;
    };
    
    public Container.ChangeFeedMonitorLeaseReleaseDelegate OnLeaseReleaseAsync => leaseToken =>
    {
        logger.LogInformation($"Lease {leaseToken} is released and processing is stopped");
        return Task.CompletedTask;
    };

    public Container.ChangeFeedMonitorErrorDelegate OnErrorAsync => (leaseToken, exception) =>
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

    public async Task HandleChangesAsync(ChangeFeedProcessorContext context, IReadOnlyCollection<JObject> changes,
        CancellationToken cancellationToken)
    {
        logger.LogInformation($"Changes detected.");
        
        foreach (var item in changes)
        {
            var tasks = auditSinks.Select(sink => sink.Consume(config, item));
            
            await Task.WhenAll(tasks);
        }
        
        logger.LogInformation($"Changes processed.");
    }
}
