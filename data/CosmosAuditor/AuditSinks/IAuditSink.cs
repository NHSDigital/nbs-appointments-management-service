using CosmosAuditor.Containers;
using Newtonsoft.Json.Linq;

namespace CosmosAuditor.AuditSinks;

public interface IAuditSink
{
    string Name { get; }
    Task Consume(ContainerConfig config, JObject item);
}
