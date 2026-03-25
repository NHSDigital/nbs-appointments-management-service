using Nhs.Appointments.Core.Metrics;

public interface IMetricsRecorder
{
    IDisposable BeginScope(string scopeName);

    void RecordMetric(string name, double value);

    void RecordMetric(string name, IMetric value);

    IReadOnlyCollection<(string Path, object Value)> Metrics { get; }
}
