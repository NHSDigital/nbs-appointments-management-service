namespace CosmosAuditor.Containers;

public record BookingContainerConfig() : ContainerConfig("booking_data", "booking_data_lease", ["ConsoleSink"]);
