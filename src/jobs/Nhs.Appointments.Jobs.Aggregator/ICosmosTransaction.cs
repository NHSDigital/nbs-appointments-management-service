namespace Nhs.Appointments.Jobs.Aggregator;

public interface ICosmosTransaction
{
    Task RunJobWithTry(Task task);
}
