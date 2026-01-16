namespace Nhs.Appointments.Jobs.BlobAuditor.Cosmos;

public class ContainerConfiguration
{
    public required string ContainerName { get; set; }
    public required string LeaseContainerName { get; set; }
}
