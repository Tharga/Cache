using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Polly;
using StackExchange.Redis;

namespace Tharga.Cache.Redis;

/// <summary>
/// Builds the resilience pipeline used by the Redis persist backend: a retry policy wrapped by a circuit breaker.
/// The breaker is the outer policy, so once it is open calls fail fast (<see cref="Polly.CircuitBreaker.BrokenCircuitException"/>)
/// without paying any retry latency — which is what prevents a sustained outage from starving the thread pool.
/// </summary>
internal static class RedisResiliencePolicy
{
    internal static IAsyncPolicy Create(RedisCacheOptions options, ILogger logger)
    {
        var retryCount = Math.Max(0, options.RetryCount);
        var retryPolicy = Policy
            .Handle<RedisException>()
            .Or<TimeoutException>()
            .Or<SocketException>()
            .WaitAndRetryAsync(
                retryCount,
                attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt)),
                (exception, timeSpan, retryCount, _) => logger?.LogWarning("Redis retry {RetryCount} after {Delay}ms due to: {Message}", retryCount, timeSpan.TotalMilliseconds, exception.Message));

        var threshold = Math.Max(1, options.CircuitBreakerFailureThreshold);
        var circuitBreakerPolicy = Policy
            .Handle<RedisException>()
            .Or<TimeoutException>()
            .Or<SocketException>()
            .CircuitBreakerAsync(
                threshold,
                options.CircuitBreakerDuration,
                onBreak: (exception, breakDelay) => logger?.LogWarning("Redis circuit opened for {Delay}ms after {Threshold} consecutive failures: {Message}", breakDelay.TotalMilliseconds, threshold, exception.Message),
                onReset: () => logger?.LogInformation("Redis circuit reset; calls are flowing to the backend again."),
                onHalfOpen: () => logger?.LogInformation("Redis circuit half-open; probing the backend with a trial call."));

        // Breaker (outer) wraps retry (inner): when the breaker is open, the retry never runs.
        return Policy.WrapAsync(circuitBreakerPolicy, retryPolicy);
    }
}
