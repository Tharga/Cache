using FluentAssertions;
using Moq;
using Tharga.Cache.Core;
using Tharga.Cache.Persist;
using Xunit;

namespace Tharga.Cache.Tests;

/// <summary>
/// Proves how <see cref="FetchQueue"/> throttles fetches, without relying on wall-clock time.
/// </summary>
/// <remarks>
/// Every fetch blocks on a <see cref="FetchGate"/> that the test opens one permit at a time, so the
/// number of fetches running at once is decided by the throttle alone and can be asserted exactly.
/// Event handlers run on several thread-pool threads at once, so they count with
/// <see cref="Interlocked"/>.
/// </remarks>
public class FetchDataThrottleTests
{
    private static readonly TimeSpan HangTimeout = TimeSpan.FromSeconds(30);

    private readonly Mock<IPersistLoader> _persistLoader = new(MockBehavior.Strict);

    [Fact]
    public async Task DualCallsWithSameKey()
    {
        //Arrange
        var options = new CacheOptions { MaxConcurrentFetchCount = 1 };
        var (sut, counts) = BuildSut(options);
        var gate = new FetchGate();

        //Act
        var task1 = sut.GetAsync("a", () => gate.RunAsync("a"), TimeSpan.FromSeconds(1));
        var task2 = sut.GetAsync("a", () => gate.RunAsync("a"), TimeSpan.FromSeconds(1));
        var task3 = sut.GetAsync("a", () => gate.RunAsync("a"), TimeSpan.FromSeconds(1));
        await gate.WaitForStartsAsync(1);
        gate.Release(3);
        var results = await Task.WhenAll(task1, task2, task3).WaitAsync(HangTimeout);

        //Assert
        results.Should().AllBe("a");
        gate.Started.Should().Be(1, "concurrent calls for the same key share one fetch");
        counts.DataSet.Should().Be(1);
        counts.DataGet.Should().Be(3);
        counts.MonitorSet.Should().Be(1);
        counts.MonitorGet.Should().Be(3);
        counts.MonitorDrop.Should().Be(0);
    }

    [Theory]
    [InlineData(2, 10)]
    [InlineData(10, 10)]
    [InlineData(10, 2)]
    public async Task ManyParallelCallsAreQueued(int fetchCount, int maxConcurrentFetchCount)
    {
        //Arrange
        var options = new CacheOptions { MaxConcurrentFetchCount = maxConcurrentFetchCount };
        var (sut, counts) = BuildSut(options);
        var gate = new FetchGate();

        //Act
        var tasks = Enumerable.Range(0, fetchCount)
            .Select(i => sut.GetAsync(Guid.NewGuid().ToString(), () => gate.RunAsync($"value-{i}"), TimeSpan.FromSeconds(1)))
            .ToArray();
        await AssertThrottledAsync(gate, fetchCount, maxConcurrentFetchCount);
        var results = await Task.WhenAll(tasks).WaitAsync(HangTimeout);

        //Assert
        results.Should().BeEquivalentTo(Enumerable.Range(0, fetchCount).Select(i => $"value-{i}"));
        counts.DataSet.Should().Be(fetchCount);
        counts.DataGet.Should().Be(fetchCount);
        counts.MonitorSet.Should().Be(fetchCount);
        counts.MonitorGet.Should().Be(fetchCount);
        counts.MonitorDrop.Should().Be(0);
    }

    [Theory]
    [InlineData(2, 10)]
    [InlineData(10, 10)]
    [InlineData(10, 2)]
    public async Task ManyParallelCallsAreQueuedForDifferentTypes(int fetchCount, int maxConcurrentFetchCount)
    {
        //Arrange
        var options = new CacheOptions { MaxConcurrentFetchCount = maxConcurrentFetchCount };
        var (sut, counts) = BuildSut(options);
        var gate = new FetchGate();

        //Act
        var intTasks = Enumerable.Range(0, fetchCount / 2)
            .Select(i => sut.GetAsync(Guid.NewGuid().ToString(), () => gate.RunAsync(i), TimeSpan.FromSeconds(1)))
            .ToArray();
        var stringTasks = Enumerable.Range(0, fetchCount / 2)
            .Select(i => sut.GetAsync(Guid.NewGuid().ToString(), () => gate.RunAsync($"value-{i}"), TimeSpan.FromSeconds(1)))
            .ToArray();
        await AssertThrottledAsync(gate, fetchCount, maxConcurrentFetchCount);
        var intResults = await Task.WhenAll(intTasks).WaitAsync(HangTimeout);
        var stringResults = await Task.WhenAll(stringTasks).WaitAsync(HangTimeout);

        //Assert
        intResults.Should().BeEquivalentTo(Enumerable.Range(0, fetchCount / 2));
        stringResults.Should().BeEquivalentTo(Enumerable.Range(0, fetchCount / 2).Select(i => $"value-{i}"));
        counts.DataSet.Should().Be(fetchCount);
        counts.DataGet.Should().Be(fetchCount);
        counts.MonitorSet.Should().Be(fetchCount);
        counts.MonitorGet.Should().Be(fetchCount);
        counts.MonitorDrop.Should().Be(0);
    }

    /// <summary>
    /// Releases the gated fetches one at a time and checks that the throttle lets exactly
    /// <paramref name="maxConcurrentFetchCount"/> run at once, across all types.
    /// </summary>
    private static async Task AssertThrottledAsync(FetchGate gate, int fetchCount, int maxConcurrentFetchCount)
    {
        var parallel = Math.Min(fetchCount, maxConcurrentFetchCount);

        // Nothing has been released, so no throttle slot can free up: exactly `parallel` fetches start.
        await gate.WaitForStartsAsync(parallel);
        gate.Started.Should().Be(parallel, "the throttle admits at most MaxConcurrentFetchCount fetches before one finishes");
        gate.Running.Should().Be(parallel);

        // Each fetch that finishes frees one slot, and the next queued fetch takes it.
        for (var started = parallel; started < fetchCount; started++)
        {
            gate.Release(1);
            await gate.WaitForStartsAsync(1);
            gate.Started.Should().Be(started + 1, "one finished fetch lets exactly one queued fetch start");
        }

        gate.Release(fetchCount);
        gate.MaxRunning.Should().Be(parallel, "the number of fetches running at once never exceeds MaxConcurrentFetchCount");
    }

    private (TimeToLiveCache Sut, EventCounts Counts) BuildSut(CacheOptions options)
    {
        var cacheMonitor = new CacheMonitor(_persistLoader.Object, options);
        var fetchQueue = new FetchQueue(cacheMonitor, options, null);
        _persistLoader.Setup(x => x.GetPersist(It.IsAny<Type>())).Returns(new Memory(cacheMonitor));
        var sut = new TimeToLiveCache(cacheMonitor, _persistLoader.Object, fetchQueue, options);

        var counts = new EventCounts();
        sut.DataGetEvent += (_, _) => Interlocked.Increment(ref counts.DataGetCount);
        sut.DataSetEvent += (_, _) => Interlocked.Increment(ref counts.DataSetCount);
        cacheMonitor.DataSetEvent += (_, _) => Interlocked.Increment(ref counts.MonitorSetCount);
        cacheMonitor.DataGetEvent += (_, _) => Interlocked.Increment(ref counts.MonitorGetCount);
        cacheMonitor.DataDropEvent += (_, _) => Interlocked.Increment(ref counts.MonitorDropCount);
        return (sut, counts);
    }

    private sealed class EventCounts
    {
        public int DataGetCount;
        public int DataSetCount;
        public int MonitorSetCount;
        public int MonitorGetCount;
        public int MonitorDropCount;

        public int DataGet => Volatile.Read(ref DataGetCount);
        public int DataSet => Volatile.Read(ref DataSetCount);
        public int MonitorSet => Volatile.Read(ref MonitorSetCount);
        public int MonitorGet => Volatile.Read(ref MonitorGetCount);
        public int MonitorDrop => Volatile.Read(ref MonitorDropCount);
    }

    /// <summary>
    /// A fetch body that records that it started, then waits for the test to release it.
    /// </summary>
    private sealed class FetchGate
    {
        private readonly SemaphoreSlim _permits = new(0);
        private readonly SemaphoreSlim _starts = new(0);
        private int _started;
        private int _running;
        private int _maxRunning;

        public int Started => Volatile.Read(ref _started);
        public int Running => Volatile.Read(ref _running);
        public int MaxRunning => Volatile.Read(ref _maxRunning);

        public async Task<T> RunAsync<T>(T value)
        {
            var running = Interlocked.Increment(ref _running);
            int max;
            while (running > (max = Volatile.Read(ref _maxRunning)) && Interlocked.CompareExchange(ref _maxRunning, running, max) != max)
            {
            }

            Interlocked.Increment(ref _started);
            _starts.Release();
            try
            {
                await _permits.WaitAsync();
            }
            finally
            {
                Interlocked.Decrement(ref _running);
            }

            return value;
        }

        public void Release(int count) => _permits.Release(count);

        /// <summary>
        /// Waits until <paramref name="count"/> more fetches have started. The timeout only guards against a hang.
        /// </summary>
        public async Task WaitForStartsAsync(int count)
        {
            for (var i = 0; i < count; i++)
            {
                if (!await _starts.WaitAsync(HangTimeout))
                {
                    throw new TimeoutException($"Expected another fetch to start, but only {Started} started within {HangTimeout}.");
                }
            }
        }
    }
}
