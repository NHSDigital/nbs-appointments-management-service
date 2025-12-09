using Microsoft.Extensions.Options;
using Nhs.Appointments.Jobs.BlobAuditor.Configuration;

namespace Nhs.Appointments.Jobs.BlobAuditor;

public class ContainerConfigFactory : IContainerConfigFactory
{
    private readonly IEnumerable<ContainerConfiguration> configurations;

    public ContainerConfigFactory(IOptions<IEnumerable<ContainerConfiguration>> configs)
    {
        this.configurations = configs.Value;
    }

    public ContainerConfiguration CreateContainerConfig(string containerName)
    {
        return configurations.First(c => c.ContainerName.Equals(containerName, StringComparison.OrdinalIgnoreCase));
    }
}
