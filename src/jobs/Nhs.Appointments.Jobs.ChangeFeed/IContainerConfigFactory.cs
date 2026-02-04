namespace Nhs.Appointments.Jobs.ChangeFeed;
public interface IContainerConfigFactory
{
    ContainerConfiguration CreateContainerConfig(string containerName);
}
