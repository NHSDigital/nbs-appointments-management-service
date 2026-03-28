using Nhs.Appointments.Core.Metrics;

public interface IMetricsRecorder
{
    IDisposable BeginScope(string scopeName);

    void RecordMetric(IMetric metric);

    IReadOnlyCollection<(string Path, IMetric Metric)> Metrics { get; }
}
