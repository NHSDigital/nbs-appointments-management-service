using Nhs.Appointments.Jobs.BlobAuditor.Configuration;

namespace Nhs.Appointments.Jobs.BlobAuditor;
public interface IContainerConfigFactory
{
    ContainerConfiguration CreateContainerConfig(string containerName);
}
