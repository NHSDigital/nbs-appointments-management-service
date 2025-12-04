namespace Nhs.Appointments.Jobs.BlobAuditor.Cosmos;

public class ContainerConfiguration
{
    public string ContainerName { get; set; }
    public string LeaseContainerName { get; set; }
}
