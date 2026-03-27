using System.Collections.Concurrent;

namespace Nhs.Appointments.Core.Metrics;

public class InMemoryMetricsRecorder : IMetricsRecorder
{
    private readonly ConcurrentStack<string> _scopeStack = new();
    private readonly List<(string Path, IMetric Metric)> _metrics = [];

    public void RecordMetric(IMetric metric)
    {
        lock (_metrics)
        {
            _scopeStack.TryPeek(out var currentName);
            var scopeName = string.IsNullOrEmpty(currentName) ? metric.Name : currentName + "/" + metric.Name;
            _metrics.Add((scopeName, metric));
        }
    }

    public IReadOnlyCollection<(string Path, IMetric Metric)> Metrics
    {
        get
        {
            lock (_metrics)
            {
                return _metrics.ToList().AsReadOnly();
            }
        }
    }

    public IDisposable BeginScope(string scopeName)
    {
        _scopeStack.TryPeek(out var currentName);
        var newScope = string.IsNullOrEmpty(currentName) ? scopeName : currentName + "/" + scopeName;
        _scopeStack.Push(newScope);

        return new InMemoryMetricsRecorderScope(this);
    }

    private class InMemoryMetricsRecorderScope(InMemoryMetricsRecorder recorder) : IDisposable
    {
        public void Dispose()
        {
            lock (recorder)
            {
                if (recorder._scopeStack.Count > 0)
                    recorder._scopeStack.TryPop(out var _);
            }
        }
    }
}
