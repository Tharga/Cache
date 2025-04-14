using FluentAssertions;
using Moq;
using Tharga.Cache.Core;
using Tharga.Cache.Persist;
using Xunit;

namespace Tharga.Cache.Tests;

public class EternalCacheTest
{
    private readonly Mock<IPersistLoader> _persistLoader = new(MockBehavior.Strict);
    private readonly Mock<IFetchQueue> _fetchQueue = new(MockBehavior.Strict);
    private readonly CacheMonitor _cacheMonitor;

    public EternalCacheTest()
    {
        _cacheMonitor = new CacheMonitor(_persistLoader.Object, new CacheOptions());
        _persistLoader.Setup(x => x.GetPersist(It.IsAny<PersistType>())).Returns(new Memory(_cacheMonitor));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task KeepAlways(bool keep)
    {
        //Arrange
        var options = new CacheOptions();
        var dataSetEventCount = 0;
        var dataGetEventCount = 0;
        var monitorDataSetEventCount = 0;
        var sut = new EternalCache(_cacheMonitor, _persistLoader.Object, _fetchQueue.Object, options);
        sut.DataSetEvent += (_, _) => dataSetEventCount++;
        sut.DataGetEvent += (_, _) => dataGetEventCount++;
        _cacheMonitor.DataSetEvent += (_, _) => monitorDataSetEventCount++;
        //Act
        _ = await sut.GetAsync("a", () => Task.FromResult("a1"));
        await Task.Delay(100);
        _ = await sut.GetAsync("a", () => Task.FromResult("a2"));
        await Task.Delay(100);
        if (keep) _ = await sut.GetAsync("a", () => Task.FromResult("a3"));
        await Task.Delay(100);
        var result = await sut.GetAsync("a", () => Task.FromResult("a4"));

        //Assert
        result.Should().Be("a1");
        dataSetEventCount.Should().Be(1);
        dataGetEventCount.Should().Be(keep ? 4 : 3);
        monitorDataSetEventCount.Should().Be(1);
    }
}