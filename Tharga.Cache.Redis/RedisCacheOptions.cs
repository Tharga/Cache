namespace Tharga.Cache.Redis;

public record RedisCacheOptions
{
    public Func<IServiceProvider, string> ConnectionStringLoader;

    /// <summary>
    /// Number of retry attempts on a transient Redis error before a call is considered failed. Default 3.
    /// Set to 0 to disable retries (each call is attempted once).
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Number of consecutive failed calls before the circuit opens and subsequent calls fail fast
    /// (throwing <see cref="Polly.CircuitBreaker.BrokenCircuitException"/>) instead of paying retry latency.
    /// Combined with <see cref="Tharga.Cache.CacheOptions.FailOpenOnBackendError"/>, an open circuit means the
    /// cache falls straight through to the source loader. Default 5.
    /// </summary>
    public int CircuitBreakerFailureThreshold { get; set; } = 5;

    /// <summary>
    /// How long the circuit stays open before it transitions to half-open and probes the backend with a
    /// single trial call. Default 30 seconds.
    /// </summary>
    public TimeSpan CircuitBreakerDuration { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Optional per-command timeout applied to the StackExchange.Redis connection
    /// (Async / Sync / Connect timeout). A shorter timeout makes the fail-open path fast even before the
    /// circuit breaker opens. When null, the connection-string / library defaults apply.
    /// </summary>
    public TimeSpan? CommandTimeout { get; set; }
}
