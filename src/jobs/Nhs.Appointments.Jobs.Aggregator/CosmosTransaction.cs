namespace Nhs.Appointments.Jobs.Aggregator;

public class CosmosTransaction : ICosmosTransaction
{
    private SemaphoreSlim _semaphore = new(1);
    public Task RunJobWithTry(Task task) => throw new NotImplementedException();
}
