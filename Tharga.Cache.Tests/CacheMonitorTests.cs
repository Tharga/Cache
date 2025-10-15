using FluentAssertions;
using Tharga.Cache.Tests.Helper;
using Xunit;

namespace Tharga.Cache.Tests;

public class CacheMonitorTests
{
    [Theory]
    [ClassData(typeof(AllTypes))]
    public async Task Set(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var result = CacheTypeLoader.GetCache(type, evictionPolicy, staleWhileRevalidate);

        //Act
        await result.Cache.SetAsync("a", 1);

        //Assert
        result.Monitor.GetInfos().SelectMany(x => x.Items).Sum(x => x.Value.Size).Should().Be(1);
    }

    [Theory]
    [ClassData(typeof(AllTypes))]
    public async Task Get(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var result = CacheTypeLoader.GetCache(type, evictionPolicy, staleWhileRevalidate);

        //Act
        await result.Cache.GetAsync("a", () => Task.FromResult(1));

        //Assert
        result.Monitor.GetInfos().SelectMany(x => x.Items).Sum(x => x.Value.Size).Should().Be(1);
    }
}