public interface IMetricsRecorder
{
    IDisposable BeginScope(string scopeName);
    void RecordMetric(string name, double value);

    void WriteMetricsToConsole();
}

public class InMemoryMetricsRecorder : IMetricsRecorder
{
    private Stack<string> _scopeStack = new Stack<string>();

    private List<(string, double)> _metrics = new List<(string, double)>();
    public void RecordMetric(string name, double value)
    {
        var scopedName = string.Join("/", _scopeStack.Reverse().Concat(new[] { name }));
        _metrics.Add((scopedName, value));
    }

    public void WriteMetricsToConsole()
    {
        foreach (var metric in _metrics)
        {
            Console.WriteLine(metric.Item1 + ": " + metric.Item2);
        }
    }

    public IDisposable BeginScope(string scopeName)
    {
        _scopeStack.Push(scopeName);
        return new InMemoryMetricsRecorderScope(this);
    }

    private class InMemoryMetricsRecorderScope(InMemoryMetricsRecorder recorder) : IDisposable
    {
        public void Dispose()
        {
            recorder._scopeStack.Pop();
        }
    }
}