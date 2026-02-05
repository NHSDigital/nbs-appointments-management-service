using System.Collections.Concurrent;

namespace Nhs.Appointments.Core.Metrics;

public class InMemoryMetricsRecorder : IMetricsRecorder
{
    private readonly ConcurrentStack<string> _scopeStack = new();
    private readonly List<(string Path, double Value)> _metrics = [];

    public void RecordMetric(string name, double value)
    {
        lock (_metrics)
        {
            _scopeStack.TryPeek(out var currentName);
            var scopeName = string.IsNullOrEmpty(currentName) ? name : currentName + "/" + name;
            _metrics.Add((scopeName, value));
        }
    }

    public IReadOnlyCollection<(string Path, double Value)> Metrics
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
