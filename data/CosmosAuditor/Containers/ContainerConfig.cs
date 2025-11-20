using Newtonsoft.Json.Linq;

namespace CosmosAuditor.Containers;

public record ContainerConfig(string Name, string LeaseName, string[] Sinks, Func<JObject, string> IdResolver);
