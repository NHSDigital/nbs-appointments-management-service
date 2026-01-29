namespace Nhs.Appointments.Jobs.Aggregator;

public interface ICosmosTransaction
{
    Task RunJobWithTry(Func<Task> action);
}
