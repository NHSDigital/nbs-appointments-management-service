public interface IMetricsRecorder
{
    IDisposable BeginScope(string scopeName);
    void RecordMetric(string name, double value);

    IReadOnlyCollection<(string Path, double Value)> Metrics { get; }
}
