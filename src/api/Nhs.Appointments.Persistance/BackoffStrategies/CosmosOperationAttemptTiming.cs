namespace Nhs.Appointments.Persistance.BackoffStrategies;

/// <summary>
/// This record holds the timings for a single attempted operation to the Cosmos db.
/// </summary>
public record CosmosOperationAttemptTiming
{
    private DateTime? startTime = null!;
    private DateTime? endTime = null!;

    internal DateTime? StartTime
    {
        get => startTime;
        set
        {
            if (startTime is not null)
            {
                throw new InvalidOperationException($"{nameof(StartTime)} cannot be set more than once.");
            }

            startTime = value;
        }
    }

    internal DateTime? EndTime
    {
        get => endTime;
        set
        {
            if (endTime is not null)
            {
                throw new InvalidOperationException($"{nameof(EndTime)} cannot be set more than once.");
            }

            if (startTime is null)
            {
                throw new InvalidOperationException($"{nameof(EndTime)} cannot be set until the {nameof(StartTime)} has been set.");
            }

            if (value < startTime)
            {
                throw new InvalidOperationException($"{nameof(EndTime)} cannot be set to an earlier value than the {nameof(StartTime)}.");
            }

            endTime = value;
        }
    }

    internal TimeSpan? Elapsed => EndTime is not null ? EndTime - StartTime : null!;

    internal bool IsStarted => StartTime is not null;
}
