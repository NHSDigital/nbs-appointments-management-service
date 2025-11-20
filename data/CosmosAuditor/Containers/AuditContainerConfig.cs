namespace CosmosAuditor.Containers;

public record AuditContainerConfig() : ContainerConfig("audit_data", "audit_data_lease", ["ConsoleSink", "BlobSink"], entity => entity.Value<string>("id"));
