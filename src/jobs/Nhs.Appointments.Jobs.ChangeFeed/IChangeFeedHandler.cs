using Microsoft.Azure.Cosmos;

namespace Nhs.Appointments.Jobs.ChangeFeed;

public interface IChangeFeedHandler
{
    Task<ChangeFeedProcessor> ResolveChangeFeedForContainer(string containerName);
}
