using CosmosAuditor.Containers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CosmosAuditor.AuditSinks;

public class ConsoleAuditSink(ILogger<ConsoleAuditSink> logger) : IAuditSink
{
    public string Name => "ConsoleSink";

    public Task Consume(ContainerConfig config, object item)
    {
        logger.LogInformation($"Detected change on Container {config.Name} - {Environment.NewLine}{JsonConvert.SerializeObject(item,  Formatting.Indented)}");
        return Task.CompletedTask;
    }
}
