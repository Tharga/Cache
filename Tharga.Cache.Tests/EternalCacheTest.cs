using FluentAssertions;
using Tharga.Cache.Core;
using Tharga.Cache.Tests.Helper;
using Xunit;

namespace Tharga.Cache.Tests;

public class EternalCacheTest
{
    private readonly MemoryPersistLoader _memoryPersistLoader;
    private readonly CacheMonitor _cacheMonitor;

    public EternalCacheTest()
    {
        _memoryPersistLoader = new MemoryPersistLoader();
        _cacheMonitor = new CacheMonitor(_memoryPersistLoader, new CacheOptions());
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
        var sut = new EternalCache(_cacheMonitor, _memoryPersistLoader, options);
        sut.DataSetEvent += (_, _) => dataSetEventCount++;
        sut.DataGetEvent += (_, _) => dataGetEventCount++;

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
    }
}