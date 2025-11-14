using CosmosAuditor.Containers;

namespace CosmosAuditor.AuditSinks;

public interface IAuditSink
{
    string Name { get; }
    Task Consume(ContainerConfig config, object item);
}
