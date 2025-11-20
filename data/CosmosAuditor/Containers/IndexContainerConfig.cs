namespace CosmosAuditor.Containers;

public record IndexContainerConfig() : ContainerConfig("index_data", "index_data_lease", ["ConsoleSink", "BlobSink"], entity => $"{entity.Value<string>("id")}-{entity.Value<string>("docType")}");
