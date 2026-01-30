namespace Nhs.Appointments.Jobs.ChangeFeed;

public class ContainerConfiguration
{
    public required string ContainerName { get; set; }
    public required string LeaseContainerName { get; set; }
}
