namespace Nhs.Appointments.Jobs.BlobAuditor.Cosmos;
public interface IContainerConfigFactory
{
    ContainerConfiguration CreateContainerConfig(string containerName);
}
