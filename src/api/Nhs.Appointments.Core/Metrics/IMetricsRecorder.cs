using Nhs.Appointments.Core.Metrics;

public interface IMetricsRecorder
{
    string Source { get; }

    void BeginRecording(string source);

    void RecordMetric(IMetric metric);

    IReadOnlyCollection<IMetric> Metrics { get; }
}
