namespace Nhs.Appointments.Persistance.BackoffStrategies;

internal record CosmosBackoffContext
{
    public Guid LinkId { get; } = Guid.NewGuid();

    public int RetryCount { get; private set; } = 0;

    public TimeSpan TotalDelayMs { get; private set; } = TimeSpan.FromMilliseconds(0);

    public bool IsReattempt => RetryCount > 0;

    public void RecordBackoff(TimeSpan nextRetryDelayMs)
    {
        RetryCount++;
        TotalDelayMs += nextRetryDelayMs;
    }
}
