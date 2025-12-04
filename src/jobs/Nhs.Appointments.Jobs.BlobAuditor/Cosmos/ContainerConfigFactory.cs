using Microsoft.Extensions.Options;

namespace Nhs.Appointments.Jobs.BlobAuditor.Cosmos;

public class ContainerConfigFactory(IOptions<List<ContainerConfiguration>> configs) : IContainerConfigFactory
{
    private readonly IEnumerable<ContainerConfiguration> configurations = configs.Value;

    public ContainerConfiguration CreateContainerConfig(string containerName)
    {
        return configurations.First(c => c.ContainerName.Equals(containerName, StringComparison.OrdinalIgnoreCase))
            ?? throw new NullReferenceException($"Container configuration not found for {containerName}");;
    }
}
