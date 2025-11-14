namespace CosmosAuditor.Containers;

public record IndexContainerConfig() : ContainerConfig("index_data", "index_data_lease", ["ConsoleSink"]);
