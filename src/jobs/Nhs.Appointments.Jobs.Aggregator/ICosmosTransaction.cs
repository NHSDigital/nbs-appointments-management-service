namespace Nhs.Appointments.Jobs.Aggregator;

public interface ICosmosTransaction
{
    Task RunJobWithTry(string transactionType, Func<Task> action);
}
