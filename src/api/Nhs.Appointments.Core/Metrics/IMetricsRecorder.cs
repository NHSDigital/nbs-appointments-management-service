using Nhs.Appointments.Core.Metrics;

public interface IMetricsRecorder
{
    IDisposable BeginScope(string scopeName);

    void RecordMetric(string name, double value);

    void RecordMetric(string name, IMetric values);

    IReadOnlyCollection<(string Path, object Value)> Metrics { get; } // TODO: Replace this with IMetric rather than object.
}
