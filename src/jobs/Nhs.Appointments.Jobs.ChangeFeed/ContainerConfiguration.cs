namespace Nhs.Appointments.Jobs.ChangeFeed;

public class ContainerConfiguration
{
    public required string ContainerName { get; set; }
    public required string LeaseContainerName { get; set; }
    public required int PollingIntervalSeconds { get; set; } = 60;
}
