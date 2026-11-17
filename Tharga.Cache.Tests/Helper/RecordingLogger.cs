using Microsoft.Extensions.Logging;

namespace Tharga.Cache.Tests.Helper;

internal class RecordingLogger<T> : ILogger<T>
{
    private readonly List<(LogLevel Level, Exception Exception, string Message)> _entries = new();

    public IReadOnlyList<(LogLevel Level, Exception Exception, string Message)> Entries
    {
        get
        {
            lock (_entries) return _entries.ToArray();
        }
    }

    public async Task<bool> WaitForAsync(Func<(LogLevel Level, Exception Exception, string Message), bool> predicate, TimeSpan timeout)
    {
        var until = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < until)
        {
            if (Entries.Any(predicate)) return true;
            await Task.Delay(10);
        }

        return Entries.Any(predicate);
    }

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        lock (_entries) _entries.Add((logLevel, exception, formatter(state, exception)));
    }
}
