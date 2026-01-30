namespace Nhs.Appointments.Jobs.ChangeFeed;

public interface ISink<in T>
{
    Task Consume(string source, T item);
}
