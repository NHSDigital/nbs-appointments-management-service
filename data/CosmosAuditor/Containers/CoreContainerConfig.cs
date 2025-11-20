namespace CosmosAuditor.Containers;

public record CoreContainerConfig() : ContainerConfig("core_data", "core_data_lease", ["ConsoleSink", "BlobSink"], entity => $"{entity.Value<string>("id")}-{entity.Value<string>("docType")}");
