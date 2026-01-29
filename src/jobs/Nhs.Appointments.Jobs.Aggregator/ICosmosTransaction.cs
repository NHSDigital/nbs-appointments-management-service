namespace Nhs.Appointments.Jobs.Aggregator;

public interface ICosmosTransaction
{
    Task RunJobWithRetry(Func<Task> action);
}
