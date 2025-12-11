using Microsoft.Extensions.Options;
using Nhs.Appointments.Jobs.BlobAuditor.Cosmos;

namespace Nhs.Appointments.Jobs.BlobAuditor.UnitTests.Cosmos;

public class ContainerConfigFactoryTests
{
    [Fact]
    public void CreateContainerConfig_ShouldReturnMatchingConfiguration()
    {
        // Arrange
        var configs = new List<ContainerConfiguration>
        {
            new ContainerConfiguration { ContainerName = "container1", LeaseContainerName = "lease1" },
            new ContainerConfiguration { ContainerName = "container2", LeaseContainerName = "lease2" }
        };

        var options = Options.Create(configs);
        var factory = new ContainerConfigFactory(options);

        // Act
        var result = factory.CreateContainerConfig("container2");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("container2", result.ContainerName);
        Assert.Equal("lease2", result.LeaseContainerName);
    }

    [Fact]
    public void CreateContainerConfig_ShouldBeCaseInsensitive()
    {
        // Arrange
        var configs = new List<ContainerConfiguration>
        {
            new ContainerConfiguration { ContainerName = "MyContainer", LeaseContainerName = "lease1" }
        };
        var options = Options.Create(configs);
        var factory = new ContainerConfigFactory(options);

        // Act
        var result = factory.CreateContainerConfig("mycontainer");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MyContainer", result.ContainerName);
    }

    [Fact]
    public void CreateContainerConfig_ShouldThrow_WhenNotFound()
    {
        // Arrange
        var configs = new List<ContainerConfiguration>
        {
            new ContainerConfiguration { ContainerName = "container1", LeaseContainerName = "lease1" }
        };
        var options = Options.Create(configs);
        var factory = new ContainerConfigFactory(options);

        // Act & Assert
        var ex = Assert.Throws<NullReferenceException>(() => factory.CreateContainerConfig("nonexistent"));
        Assert.Equal("Container configuration not found for nonexistent", ex.Message);
    }
}
