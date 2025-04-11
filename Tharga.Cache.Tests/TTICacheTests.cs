using FluentAssertions;
using Moq;
using Tharga.Cache.Core;
using Xunit;

namespace Tharga.Cache.Tests;

public class TTICacheTests
{
    private readonly Mock<IPersistLoader> _persistLoader = new(MockBehavior.Strict);
    private readonly CacheMonitor _cacheMonitor;

    public TTICacheTests()
    {
        _cacheMonitor = new CacheMonitor(_persistLoader.Object, new CacheOptions());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task KeepIfUsed(bool keep)
    {
        //Arrange
        var options = new CacheOptions();
        var dataSetEventCount = 0;
        var dataGetEventCount = 0;
        var sut = new TimeToIdleCache(_cacheMonitor, _persistLoader.Object, options);
        sut.DataSetEvent += (_, _) => dataSetEventCount++;
        sut.DataGetEvent += (_, _) => dataGetEventCount++;

        //Act
        _ = await sut.GetAsync("a", () => Task.FromResult("a1"), TimeSpan.FromMilliseconds(200));
        await Task.Delay(100);
        _ = await sut.GetAsync("a", () => Task.FromResult("a2"), TimeSpan.FromMilliseconds(200));
        await Task.Delay(100);
        if (keep) _ = await sut.GetAsync("a", () => Task.FromResult("a3"), TimeSpan.FromMilliseconds(200));
        await Task.Delay(100);
        var result = await sut.GetAsync("a", () => Task.FromResult("a4"), TimeSpan.FromMilliseconds(200));

        //Assert
        result.Should().Be(keep ? "a1" : "a4");
        dataSetEventCount.Should().Be(keep ? 1 : 2);
        dataGetEventCount.Should().Be(keep ? 4 : 3);
    }
}