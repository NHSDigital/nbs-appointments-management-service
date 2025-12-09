using Newtonsoft.Json.Linq;

namespace Nhs.Appointments.Jobs.BlobAuditor.Sink;

public interface IBlobSink<T>
{
    Task Consume(string source, T item);
}
