namespace Nhs.Appointments.Core.Metrics;

public class InMemoryMetricsRecorder : IMetricsRecorder
{
    private readonly List<IMetric> _metrics = [];

    public string Source { get; private set; } = null!;

    public void RecordMetric(IMetric metric)
    {
        lock (_metrics)
        {
            _metrics.Add(metric);
        }
    }

    public void BeginRecording(string source)
    {
        ArgumentException.ThrowIfNullOrEmpty(source, nameof(source));

        if (Source is not null)
        {
            throw new InvalidOperationException("Source cannot be set more than once.");
        }

        Source = source;
    }

    public IReadOnlyCollection<IMetric> Metrics
    {
        get
        {
            lock (_metrics)
            {
                return _metrics.ToList().AsReadOnly();
            }
        }
    }
}
