namespace Nhs.Appointments.Core.Concurrency;

public class SiteLeaseManagerOptions
{
    public TimeSpan Timeout { get; set; }
    public string ContainerName { get; set; }
}
