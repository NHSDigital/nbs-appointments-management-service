using Microsoft.Extensions.Options;

namespace Nhs.Appointments.Jobs.ChangeFeed;

public class ContainerConfigFactory(IOptions<List<ContainerConfiguration>> configs) : IContainerConfigFactory
{
    private readonly IEnumerable<ContainerConfiguration> configurations = configs.Value;

    public ContainerConfiguration CreateContainerConfig(string containerName)
    {
        return configurations.FirstOrDefault(c => c.ContainerName.Equals(containerName, StringComparison.OrdinalIgnoreCase))
            ?? throw new NullReferenceException($"Container configuration not found for {containerName}");
    }
}
