using Nhs.Appointments.Core.Metrics;
using Nhs.Appointments.Persistance.BackoffStrategies;

namespace Nhs.Appointments.Persistance;

/// <summary>
/// This record holds the set of metrics to be logged when interacting with the Cosmos db.
/// </summary>
public record CosmosOperationMetric : IMetric
{
    private CosmosOperationAttemptTiming Current => Timings.LastOrDefault();

    public DateTime? StartTime { get; private set; }

    public DateTime? EndTime { get; private set; }

    public double RuCharge { get; private set; } = 0.0;

    public string Container { get; init; }

    public string DocumentType { get; init; }

    public string Path { get; init; }

    public List<CosmosOperationAttemptTiming> Timings { get; private set; } = [];

    public string Name => nameof(CosmosOperationMetric);

    internal void StartAttempt(DateTime startTime)
    {
        if (Current is not null && Current.StartTime is not null && Current.EndTime is null)
        {
            throw new InvalidOperationException($"{nameof(StartAttempt)} cannot be called before {nameof(EndAttempt)}");
        }

        StartTime ??= startTime;

        Timings.Add(new CosmosOperationAttemptTiming { StartTime = startTime });

        EndTime = null;
    }

    internal void EndAttempt(DateTime endTime)
    {
        if (Current == null || Current.EndTime is not null)
        {
            throw new InvalidOperationException($"{nameof(EndAttempt)} cannot be called before {nameof(StartAttempt)}");
        }
        
        Current.EndTime = endTime;
        EndTime = endTime;
    }

    internal void AddRuCharge(double ruCharge)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(ruCharge, 0);

        RuCharge += ruCharge;
    }
}
