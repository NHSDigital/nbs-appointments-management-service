namespace CosmosAuditor.Containers;

public record BookingContainerConfig() : ContainerConfig("booking_data", "booking_data_lease", ["ConsoleSink", "BlobSink"], entity => $"{entity.Value<string>("id")}-{entity.Value<string>("site")}");
