namespace Nhs.Appointments.Jobs.BlobAuditor.Sink;

public interface IBlobSink<in T>
{
    Task Consume(string source, T item);
}
