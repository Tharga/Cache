using FluentAssertions;
using Moq;
using Tharga.Cache.Core;
using Tharga.Cache.Persist;
using Xunit;

namespace Tharga.Cache.Tests;

public class TimeToLiveCacheTests
{
    private readonly Mock<IPersistLoader> _persistLoader = new(MockBehavior.Strict);
    private readonly CacheMonitor _cacheMonitor;
    private readonly IFetchQueue _fetchQueue;

    public TimeToLiveCacheTests()
    {
        var options = new CacheOptions();
        _cacheMonitor = new CacheMonitor(_persistLoader.Object, options);
        _persistLoader.Setup(x => x.GetPersist(It.IsAny<PersistType>())).Returns(new Memory(_cacheMonitor));
        _fetchQueue = new FetchQueue(_cacheMonitor, options, null);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DropEvenIfUsed(bool keep)
    {
        //Arrange
        var options = new CacheOptions();
        var dataSetEventCount = 0;
        var dataGetEventCount = 0;
        var monitorSetEventCount = 0;
        var monitorGetEventCount = 0;
        var monitorDropEventCount = 0;
        var sut = new TimeToLiveCache(_cacheMonitor, _persistLoader.Object, _fetchQueue, options);
        sut.DataSetEvent += (_, _) => dataSetEventCount++;
        sut.DataGetEvent += (_, _) => dataGetEventCount++;
        _cacheMonitor.DataSetEvent += (_, _) => { monitorSetEventCount++; };
        _cacheMonitor.DataGetEvent += (_, _) => { monitorGetEventCount++; };
        _cacheMonitor.DataDropEvent += (_, _) => { monitorDropEventCount++; };

        //Act
        _ = await sut.GetAsync("a", () => Task.FromResult("a1"), TimeSpan.FromMilliseconds(200));
        await Task.Delay(100);
        _ = await sut.GetAsync("a", () => Task.FromResult("a2"), TimeSpan.FromMilliseconds(200));
        await Task.Delay(100);
        if (keep) _ = await sut.GetAsync("a", () => Task.FromResult("a3"), TimeSpan.FromMilliseconds(200));
        await Task.Delay(100);
        var result = await sut.GetAsync("a", () => Task.FromResult("a4"), TimeSpan.FromMilliseconds(200));

        //Assert
        result.Should().Be(keep ? "a3" : "a4");
        dataSetEventCount.Should().Be(2);
        dataGetEventCount.Should().Be(keep ? 4 : 3);
        monitorSetEventCount.Should().Be(2);
        monitorGetEventCount.Should().Be(keep ? 4 : 3);
        monitorDropEventCount.Should().Be(0);
    }
}