public class InMemoryMetricsRecorder : IMetricsRecorder
{
    private Stack<string> _scopeStack = new Stack<string>();
    private List<(string Path, double Value)> _metrics = new List<(string Path, double Value)>();

    public void RecordMetric(string name, double value)
    {
        lock (_metrics)
        {
            var scopedName = string.Join("/", _scopeStack.Reverse().Concat(new[] { name }));
            _metrics.Add((scopedName, value));
        }
    }

    public IReadOnlyCollection<(string Path, double Value)> Metrics => _metrics.ToList().AsReadOnly();

    public IDisposable BeginScope(string scopeName)
    {
        _scopeStack.Push(scopeName);
        return new InMemoryMetricsRecorderScope(this);
    }

    private class InMemoryMetricsRecorderScope(InMemoryMetricsRecorder recorder) : IDisposable
    {
        public void Dispose()
        {
            if(recorder._scopeStack.Count > 0) 
                recorder._scopeStack.Pop();
        }
    }
}