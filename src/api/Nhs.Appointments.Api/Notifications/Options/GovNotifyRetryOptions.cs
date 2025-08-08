namespace Nhs.Appointments.Api.Notifications.Options;

public class GovNotifyRetryOptions
{
    public int MaxRetries { get; set; } = 3;
    public int InitialDelayMs { get; set; } = 500;
    public double BackoffFactor { get; set; } = 2.0;
}
