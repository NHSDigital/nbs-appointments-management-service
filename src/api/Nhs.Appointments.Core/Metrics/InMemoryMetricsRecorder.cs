using System.Collections.Concurrent;

public class InMemoryMetricsRecorder : IMetricsRecorder
{
    private ConcurrentStack<string> _scopeStack = new ConcurrentStack<string>();
    private List<(string Path, double Value)> _metrics = new List<(string Path, double Value)>();

    public void RecordMetric(string name, double value)
    {
        lock (_metrics)
        {
            _scopeStack.TryPeek(out var currentName);
            var scopedName = string.Join("/", [currentName, name]);
            _metrics.Add((scopedName, value));
        }
    }

    public IReadOnlyCollection<(string Path, double Value)> Metrics => _metrics.ToList().AsReadOnly();

    public IDisposable BeginScope(string scopeName)
    {
        _scopeStack.TryPeek(out var currentName);
        var newScope = String.IsNullOrEmpty(currentName) ? scopeName : currentName + "/" + scopeName;
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