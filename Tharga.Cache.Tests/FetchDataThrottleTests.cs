using System.Diagnostics;
using FluentAssertions;
using Moq;
using Tharga.Cache.Core;
using Tharga.Cache.Persist;
using Xunit;

namespace Tharga.Cache.Tests;

public class FetchDataThrottleTests
{
    //private readonly IFetchQueue _fetchQueue;
    private readonly Mock<IPersistLoader> _persistLoader;
    //private readonly CacheMonitor _cacheMonitor;

    public FetchDataThrottleTests()
    {
        _persistLoader = new Mock<IPersistLoader>(MockBehavior.Strict);
    }

    [Fact]
    public async Task DualCallsWithSameKey()
    {
        //Arrange
        var options = new CacheOptions();
        var dataGetEventCount = 0;
        var dataSetEventCount = 0;
        var monitorSetEventCount = 0;
        var monitorGetEventCount = 0;
        var monitorDropEventCount = 0;
        var cacheMonitor = new CacheMonitor(_persistLoader.Object, options);
        var fetchQueue = new FetchQueue(cacheMonitor, options, default);
        _persistLoader.Setup(x => x.GetPersist(options.Get<string>().PersistType)).Returns(new Memory(cacheMonitor));
        var sut = new TimeToLiveCache(cacheMonitor, _persistLoader.Object, fetchQueue, options);
        sut.DataGetEvent += (_, _) => dataGetEventCount++;
        sut.DataSetEvent += (_, _) => dataSetEventCount++;
        cacheMonitor.DataSetEvent += (_, _) => monitorSetEventCount++;
        cacheMonitor.DataGetEvent += (_, _) => monitorGetEventCount++;
        cacheMonitor.DataDropEvent += (_, _) => monitorDropEventCount++;

        //Act
        var task1 = sut.GetAsync("a", async () => { await Task.Delay(400); return "a"; }, TimeSpan.FromSeconds(1));
        var task2 = sut.GetAsync("a", async () => { await Task.Delay(400); return "a"; }, TimeSpan.FromSeconds(1));
        var task3 = sut.GetAsync("a", async () => { await Task.Delay(400); return "a"; }, TimeSpan.FromSeconds(1));
        await Task.WhenAll(task1, task2, task3);

        //Assert
        dataSetEventCount.Should().Be(1);
        dataGetEventCount.Should().Be(3);
        monitorSetEventCount.Should().Be(1);
        monitorGetEventCount.Should().Be(3);
        monitorDropEventCount.Should().Be(0);
    }

    [Theory]
    [InlineData(2, 10, 100, 200)]
    [InlineData(10, 10, 100, 200)]
    [InlineData(10, 2, 500, 900)]
    [Trait("Category", "TimeCritical")]
    public async Task ManyParallelCallsAreQueued(int fetchCount, int maxConcurrentFetchCount, int minTime, int maxTime)
    {
        //Arrange
        var options = new CacheOptions{ MaxConcurrentFetchCount = maxConcurrentFetchCount };
        var dataGetEventCount = 0;
        var dataSetEventCount = 0;
        var monitorSetEventCount = 0;
        var monitorGetEventCount = 0;
        var monitorDropEventCount = 0;
        var cacheMonitor = new CacheMonitor(_persistLoader.Object, options);
        var fetchQueue = new FetchQueue(cacheMonitor, options, default);
        _persistLoader.Setup(x => x.GetPersist(options.Get<string>().PersistType)).Returns(new Memory(cacheMonitor));
        var sut = new TimeToLiveCache(cacheMonitor, _persistLoader.Object, fetchQueue, options);
        sut.DataGetEvent += (_, _) => dataGetEventCount++;
        sut.DataSetEvent += (_, _) => dataSetEventCount++;
        cacheMonitor.DataSetEvent += (_, _) => monitorSetEventCount++;
        cacheMonitor.DataGetEvent += (_, _) => monitorGetEventCount++;
        cacheMonitor.DataDropEvent += (_, _) => monitorDropEventCount++;
        var stopwatch = Stopwatch.StartNew();

        //Act
        var tasks = Enumerable.Range(0, fetchCount).Select(_ => sut.GetAsync(Guid.NewGuid().ToString(), async () =>
        {
            await Task.Delay(100);
            return Guid.NewGuid().ToString();
        }, TimeSpan.FromSeconds(1))).ToArray();
        await Task.WhenAll(tasks);

        //Assert
        dataSetEventCount.Should().BeGreaterThanOrEqualTo(fetchCount-1);
        dataGetEventCount.Should().Be(fetchCount);
        monitorSetEventCount.Should().BeGreaterThanOrEqualTo(fetchCount-1);
        monitorGetEventCount.Should().Be(fetchCount);
        monitorDropEventCount.Should().Be(0);
        stopwatch.Elapsed.TotalMilliseconds.Should().BeGreaterThan(minTime);
        stopwatch.Elapsed.TotalMilliseconds.Should().BeLessThan(maxTime);
    }

    [Theory]
    [InlineData(2, 10, 100, 200)]
    [InlineData(10, 10, 100, 200)]
    [InlineData(10, 2, 500, 1200)]
    [Trait("Category", "TimeCritical")]
    public async Task ManyParallelCallsAreQueuedForDifferentTypes(int fetchCount, int maxConcurrentFetchCount, int minTime, int maxTime)
    {
        //Arrange
        var options = new CacheOptions { MaxConcurrentFetchCount = maxConcurrentFetchCount };
        var dataGetEventCount = 0;
        var dataSetEventCount = 0;
        var monitorSetEventCount = 0;
        var monitorGetEventCount = 0;
        var monitorDropEventCount = 0;
        var cacheMonitor = new CacheMonitor(_persistLoader.Object, options);
        var fetchQueue = new FetchQueue(cacheMonitor, options, default);
        _persistLoader.Setup(x => x.GetPersist(options.Get<string>().PersistType)).Returns(new Memory(cacheMonitor));
        var sut = new TimeToLiveCache(cacheMonitor, _persistLoader.Object, fetchQueue, options);
        sut.DataGetEvent += (_, _) => dataGetEventCount++;
        sut.DataSetEvent += (_, _) => dataSetEventCount++;
        cacheMonitor.DataSetEvent += (_, _) => monitorSetEventCount++;
        cacheMonitor.DataGetEvent += (_, _) => monitorGetEventCount++;
        cacheMonitor.DataDropEvent += (_, _) => monitorDropEventCount++;
        var stopwatch = Stopwatch.StartNew();

        //Act
        var intTasks = Enumerable.Range(0, fetchCount / 2).Select(_ => sut.GetAsync(Guid.NewGuid().ToString(), async () =>
        {
            await Task.Delay(100);
            return 1;
        }, TimeSpan.FromSeconds(1))).ToArray();
        var stringTasks = Enumerable.Range(0, fetchCount / 2).Select(_ => sut.GetAsync(Guid.NewGuid().ToString(), async () =>
        {
            await Task.Delay(100);
            return Guid.NewGuid().ToString();
        }, TimeSpan.FromSeconds(1))).ToArray();
        await Task.WhenAll(intTasks);
        await Task.WhenAll(stringTasks);

        //Assert
        dataSetEventCount.Should().BeGreaterThanOrEqualTo(fetchCount-1);
        dataGetEventCount.Should().Be(fetchCount);
        monitorSetEventCount.Should().Be(fetchCount);
        monitorGetEventCount.Should().Be(fetchCount);
        monitorDropEventCount.Should().Be(0);
        stopwatch.Elapsed.TotalMilliseconds.Should().BeGreaterThan(minTime);
        stopwatch.Elapsed.TotalMilliseconds.Should().BeLessThan(maxTime);
    }
}