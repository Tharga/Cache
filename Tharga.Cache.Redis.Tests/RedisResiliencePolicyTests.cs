using FluentAssertions;
using Polly;
using Polly.CircuitBreaker;
using Xunit;

namespace Tharga.Cache.Redis.Tests;

public class RedisResiliencePolicyTests
{
    [Fact]
    public async Task Circuit_opens_after_threshold_and_then_fails_fast_without_invoking_backend()
    {
        //Arrange — no retry so the breaker trips deterministically after exactly `threshold` failures.
        var options = new RedisCacheOptions
        {
            RetryCount = 0,
            CircuitBreakerFailureThreshold = 2,
            CircuitBreakerDuration = TimeSpan.FromMinutes(1)
        };
        var policy = RedisResiliencePolicy.Create(options, null);

        var invocations = 0;
        Func<Task> failing = async () =>
        {
            invocations++;
            await Task.Yield();
            throw new TimeoutException("simulated outage");
        };

        //Act — trip the breaker.
        for (var i = 0; i < options.CircuitBreakerFailureThreshold; i++)
        {
            await policy.Invoking(p => p.ExecuteAsync(failing)).Should().ThrowAsync<TimeoutException>();
        }

        var invocationsBeforeOpen = invocations;

        //Assert — the circuit is now open: the next call fails fast without touching the backend.
        await policy.Invoking(p => p.ExecuteAsync(failing)).Should().ThrowAsync<BrokenCircuitException>();
        invocations.Should().Be(invocationsBeforeOpen, "an open circuit must short-circuit instead of invoking the backend");
    }

    [Fact]
    public async Task Successful_call_passes_through()
    {
        //Arrange
        var policy = RedisResiliencePolicy.Create(new RedisCacheOptions { RetryCount = 0, CircuitBreakerFailureThreshold = 5 }, null);

        //Act
        var result = await policy.ExecuteAsync(() => Task.FromResult(42));

        //Assert
        result.Should().Be(42);
    }

    [Fact]
    public async Task Retries_transient_failures_then_succeeds()
    {
        //Arrange
        var options = new RedisCacheOptions { RetryCount = 2, CircuitBreakerFailureThreshold = 5 };
        var policy = RedisResiliencePolicy.Create(options, null);

        var attempts = 0;

        //Act
        var result = await policy.ExecuteAsync(async () =>
        {
            attempts++;
            await Task.Yield();
            if (attempts < 3) throw new TimeoutException("transient");
            return "ok";
        });

        //Assert
        result.Should().Be("ok");
        attempts.Should().Be(3, "the call should be retried twice before succeeding on the third attempt");
    }
}
