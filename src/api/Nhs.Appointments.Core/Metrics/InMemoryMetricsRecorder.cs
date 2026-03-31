using System.Collections.Concurrent;

namespace Nhs.Appointments.Core.Metrics;

public class InMemoryMetricsRecorder : IMetricsRecorder
{
    private readonly ConcurrentStack<string> _scopeStack = new();
    private readonly List<(string Path, IMetric Metric)> _metrics = [];

    public void RecordMetric(IMetric metric)
    {
        // This will block async threads that are trying to record their metrics.
        // For performance, the lock should be replaced with a SemaphoreSlim.WaitAsync() and this method be made asynchronous, however see note below about BeginScope.
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
        // The newScope added to the stack (and therefore the Path) may not accurately represent the scope, since the scopeName values are being added to the stack from asynchronous methods.
        // The initial scope is likely to be added on the main thread and will run synchronously, but after that subsequent scope calls could be called from async methods and can 
        // therefore run at an indeterminate time.
        // The problem is this method may be called twice in quick succession by two methods which are siblings, but which will appear (given the scope construct) to be parent/child.
        // When any metrics are added, they will be associated with the Path at the TOP of the stack, which may be wrong.
        // e.g.:
        // async method 1 is called and adds scope A
        // async method 2 is called and adds scope B
        // async method 1 completes and adds its metric to the current scope which is scope B.
        // It may be possible to add a Guid to the scope being added to the stack, and retain it in the returned scope.
        // Calls made within that scope can then pass the Guid down to the database access code in order for the metric to be recorded against the correct Guid.
        // This essentially obviates the choice of a stack, and it could become a bag.
        // HOWEVER, the problem of accurately determining the path remains as it is dependent on when the Dispose is called for the scope.
        // Can we solve this problem by including the ThreadId as part of the identifier for the path? NO - it changes each time and the BeginScope calls are initially made on the main thread.
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
