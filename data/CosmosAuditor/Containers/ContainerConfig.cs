namespace CosmosAuditor.Containers;

public record ContainerConfig(string Name, string LeaseName, string[] Sinks);
