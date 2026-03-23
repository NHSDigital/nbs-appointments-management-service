using Nhs.Appointments.Core.Metrics;
using Nhs.Appointments.Persistance.BackoffStrategies;

namespace Nhs.Appointments.Persistance;

/// <summary>
/// This record holds the set of metrics to be logged when interacting with the Cosmos db.
/// </summary>
public record CosmosOperationMetric : IMetric
{
    private readonly List<CosmosOperationAttemptTiming> timings = [];

    private CosmosOperationAttemptTiming Current => timings.LastOrDefault();

    public DateTime? StartTime { get; private set; }

    public DateTime? EndTime { get; private set; }

    public double RuCharge { get; private set; } = 0.0;

    public string Container { get; init; }

    public string DocumentType { get; init; }

    public List<CosmosOperationAttemptTiming> Timings => timings.Where(t => t.IsStarted).ToList();

    public string Name => nameof(CosmosOperationMetric);

    internal void StartAttempt(DateTime startTime)
    {
        StartTime ??= startTime;

        if (Current == null) // First time through.
        {
            timings.Add(new CosmosOperationAttemptTiming { StartTime = startTime });
        }
        else
        {
            Current.StartTime = startTime;
        }

        EndTime = null;
    }

    internal void EndAttempt(DateTime endTime)
    {
        if (Current == null)
        {
            throw new InvalidOperationException($"{nameof(EndAttempt)} cannot be called before {nameof(StartAttempt)}");
        }

        Current.EndTime = endTime;
        timings.Add(new CosmosOperationAttemptTiming());
        EndTime = endTime;
    }

    internal void AddRuCharge(double ruCharge)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(ruCharge, 0);

        RuCharge += ruCharge;
    }
}
