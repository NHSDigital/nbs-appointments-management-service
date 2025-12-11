using Microsoft.Azure.Cosmos;

namespace Nhs.Appointments.Jobs.BlobAuditor.ChangeFeed;

public interface IAuditChangeFeedHandler<T>
{
    Task<ChangeFeedProcessor> ResolveChangeFeedForContainer(string containerName);
}
