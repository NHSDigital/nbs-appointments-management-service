
namespace Nhs.Appointments.Jobs.BlobAuditor.Configuration;

public class ContainerConfiguration
{
    public string ContainerName { get; set; }
    public string LeaseContainerName { get; set; }
}
