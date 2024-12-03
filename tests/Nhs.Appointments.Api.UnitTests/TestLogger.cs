using Microsoft.Extensions.Logging;

namespace Nhs.Appointments.Api.Tests;

public class TestLogger : ILogger
{
    private readonly List<LogEntry> _logs = new List<LogEntry>();

    public IDisposable BeginScope<TState>(TState state)
    {
        throw new NotImplementedException();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        throw new NotImplementedException();
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        var formattedLogEntries = state as IReadOnlyList<KeyValuePair<string, object>>;
        _logs.Add(new LogEntry(logLevel, formattedLogEntries.First().Value.ToString(), exception));
    }

    public IReadOnlyCollection<LogEntry> LogEntries => _logs;

    public record LogEntry(LogLevel LogLevel, string Message, Exception Exception);
}
