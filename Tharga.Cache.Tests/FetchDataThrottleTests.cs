using System.Diagnostics;
using FluentAssertions;
using Moq;
using Tharga.Cache.Core;
using Tharga.Cache.Tests.Helper;
using Xunit;

namespace Tharga.Cache.Tests;

public class FetchDataThrottleTests
{
    [Fact]
    public async Task DualCallsWithSameKey()
    {
        //Arrange
        var options = new CacheOptions();
        var dataGetEventCount = 0;
        var dataSetEventCount = 0;
        var persistLoader = new Mock<IPersistLoader>(MockBehavior.Strict);
        var cacheMonitor = new CacheMonitor(persistLoader.Object, options);
        var sut = new TimeToLiveCache(cacheMonitor, persistLoader.Object, options);
        sut.DataGetEvent += (_, _) => dataGetEventCount++;
        sut.DataSetEvent += (_, _) => dataSetEventCount++;

        //Act
        var task1 = sut.GetAsync("a", async () => { await Task.Delay(400); return "a"; }, TimeSpan.FromSeconds(1));
        var task2 = sut.GetAsync("a", async () => { await Task.Delay(400); return "a"; }, TimeSpan.FromSeconds(1));
        var task3 = sut.GetAsync("a", async () => { await Task.Delay(400); return "a"; }, TimeSpan.FromSeconds(1));
        await Task.WhenAll(task1, task2, task3);

        //Assert
        dataSetEventCount.Should().Be(1);
        dataGetEventCount.Should().Be(3);
    }

    [Theory(Skip = "TimeCritical")]
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
        var persistLoader = new Mock<IPersistLoader>(MockBehavior.Strict);
        var cacheMonitor = new CacheMonitor(persistLoader.Object, options);
        var sut = new TimeToLiveCache(cacheMonitor, persistLoader.Object, options);
        sut.DataGetEvent += (_, _) => dataGetEventCount++;
        sut.DataSetEvent += (_, _) => dataSetEventCount++;
        var stopwatch = Stopwatch.StartNew();

        //Act
        var tasks = Enumerable.Range(0, fetchCount).Select(_ => sut.GetAsync(Guid.NewGuid().ToString(), async () =>
        {
            await Task.Delay(100);
            return Guid.NewGuid().ToString();
        }, TimeSpan.FromSeconds(1))).ToArray();
        await Task.WhenAll(tasks);

        //Assert
        dataSetEventCount.Should().Be(fetchCount);
        dataGetEventCount.Should().Be(fetchCount);
        stopwatch.Elapsed.TotalMilliseconds.Should().BeGreaterThan(minTime);
        stopwatch.Elapsed.TotalMilliseconds.Should().BeLessThan(maxTime);
    }

    [Theory(Skip = "TimeCritical")]
    [InlineData(2, 10, 100, 200)]
    [InlineData(10, 10, 100, 200)]
    [InlineData(10, 2, 500, 900)]
    [Trait("Category", "TimeCritical")]
    public async Task ManyParallelCallsAreQueuedForDifferentTypes(int fetchCount, int maxConcurrentFetchCount, int minTime, int maxTime)
    {
        //Arrange
        var options = new CacheOptions { MaxConcurrentFetchCount = maxConcurrentFetchCount };
        var dataGetEventCount = 0;
        var dataSetEventCount = 0;
        var persistLoader = new Mock<IPersistLoader>(MockBehavior.Strict);
        var cacheMonitor = new CacheMonitor(persistLoader.Object, options);
        var sut = new TimeToLiveCache(cacheMonitor, persistLoader.Object, options);
        sut.DataGetEvent += (_, _) => dataGetEventCount++;
        sut.DataSetEvent += (_, _) => dataSetEventCount++;
        var stopwatch = Stopwatch.StartNew();

        //Act
        var intTasks = Enumerable.Range(0, fetchCount / 2).Select(_ => sut.GetAsync<int>(Guid.NewGuid().ToString(), async () =>
        {
            await Task.Delay(100);
            return 1;
        }, TimeSpan.FromSeconds(1))).ToArray();
        var stringTasks = Enumerable.Range(0, fetchCount / 2).Select(_ => sut.GetAsync<string>(Guid.NewGuid().ToString(), async () =>
        {
            await Task.Delay(100);
            return Guid.NewGuid().ToString();
        }, TimeSpan.FromSeconds(1))).ToArray();
        await Task.WhenAll(intTasks);
        await Task.WhenAll(stringTasks);

        //Assert
        dataSetEventCount.Should().Be(fetchCount);
        dataGetEventCount.Should().Be(fetchCount);
        stopwatch.Elapsed.TotalMilliseconds.Should().BeGreaterThan(minTime);
        stopwatch.Elapsed.TotalMilliseconds.Should().BeLessThan(maxTime);
    }
}