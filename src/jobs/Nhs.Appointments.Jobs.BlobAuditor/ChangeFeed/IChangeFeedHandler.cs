using Microsoft.Azure.Cosmos;

namespace Nhs.Appointments.Jobs.BlobAuditor.ChangeFeed;

public interface IChangeFeedHandler<T>
{
    Container.ChangeFeedMonitorLeaseAcquireDelegate OnLeaseAcquiredAsync { get; }
    Container.ChangeFeedMonitorLeaseReleaseDelegate OnLeaseReleaseAsync { get; }
    Container.ChangeFeedMonitorErrorDelegate OnErrorAsync { get; }

    Task HandleChangesAsync(ChangeFeedProcessorContext context, IReadOnlyCollection<T> changes,
        CancellationToken cancellationToken);
}
