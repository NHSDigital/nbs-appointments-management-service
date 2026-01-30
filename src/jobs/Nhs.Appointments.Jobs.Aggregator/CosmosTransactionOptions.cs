namespace Nhs.Appointments.Jobs.Aggregator;

public class CosmosTransactionOptions
{
    public int MaxRetry { get; set; }
    public int DefaultWaitSeconds { get; set; }
}
