using FluentAssertions;
using Moq;
using Tharga.Cache.Core;
using Xunit;

namespace Tharga.Cache.Tests;

public class UnloadDataTests
{
    private readonly Mock<IPersistLoader> _persistLoader = new(MockBehavior.Strict);
    private readonly CacheMonitor _cacheMonitor;

    public UnloadDataTests()
    {
        _cacheMonitor = new CacheMonitor(_persistLoader.Object, new CacheOptions());
    }

    [Fact]
    public async Task FirstInFirstOut_MaxCount()
    {
        //Arrange
        var dataDropEventCount = 0;
        var options = new CacheOptions();
        options.RegisterType<string>(o =>
        {
            o.MaxCount = 3;
            o.EvictionPolicy = EvictionPolicy.FirstInFirstOut;
        });
        var sut = new EternalCache(_cacheMonitor, _persistLoader.Object, options);
        sut.DataDropEvent += (_, _) => dataDropEventCount++;
        await sut.SetAsync("a", "aa");
        await sut.SetAsync("b", "bb");
        await sut.SetAsync("c", "cc");

        //Act
        await sut.SetAsync("d", "dd");

        //Assert
        dataDropEventCount.Should().Be(1);
        _cacheMonitor.GetInfos().Single().Items.Count.Should().Be(3);
        _cacheMonitor.GetInfos().Single().Items.Should().Contain(x => x.Key == KeyBuilder.BuildKey<string>("d"));
        _cacheMonitor.GetInfos().Single().Items.Should().NotContain(x => x.Key == KeyBuilder.BuildKey<string>("a"));
        _cacheMonitor.GetInfos().SelectMany(x => x.Items).Sum(x => x.Value.Size).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task FirstInFirstOut_LeastRecentlyUsed()
    {
        //Arrange
        var dataDropEventCount = 0;
        var options = new CacheOptions();
        options.RegisterType<string>(o =>
        {
            o.MaxCount = 3;
            o.EvictionPolicy = EvictionPolicy.LeastRecentlyUsed;
        });
        var sut = new EternalCache(_cacheMonitor, _persistLoader.Object, options);
        sut.DataDropEvent += (_, _) => dataDropEventCount++;
        await sut.GetAsync("a", () => Task.FromResult("aa"));
        await sut.GetAsync("b", () => Task.FromResult("bb"));
        await sut.GetAsync("c", () => Task.FromResult("cc"));
        await sut.PeekAsync<string>("a"); //NOTE: Access "a", so it should not be evicted.

        //Act
        await sut.SetAsync("d", "dd");

        //Assert
        dataDropEventCount.Should().Be(1);
        _cacheMonitor.GetInfos().Single().Items.Count.Should().Be(3);
        _cacheMonitor.GetInfos().Single().Items.Should().Contain(x => x.Key == KeyBuilder.BuildKey<string>("b"));
        _cacheMonitor.GetInfos().Single().Items.Should().NotContain(x => x.Key == KeyBuilder.BuildKey<string>("a"));
        _cacheMonitor.GetInfos().SelectMany(x => x.Items).Sum(x => x.Value.Size).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task FirstInFirstOut_RandomReplacement()
    {
        //Arrange
        var dataDropEventCount = 0;
        var options = new CacheOptions();
        options.RegisterType<string>(o =>
        {
            o.MaxCount = 3;
            o.EvictionPolicy = EvictionPolicy.RandomReplacement;
        });
        var sut = new EternalCache(_cacheMonitor, _persistLoader.Object, options);
        sut.DataDropEvent += (_, _) => dataDropEventCount++;
        await sut.SetAsync("a", "aa");
        await sut.SetAsync("b", "bb");
        await sut.SetAsync("c", "cc");

        //Act
        await sut.SetAsync("d", "dd");

        //Assert
        dataDropEventCount.Should().Be(1);
        _cacheMonitor.GetInfos().Single().Items.Count.Should().Be(3);
        _cacheMonitor.GetInfos().Single().Items.Should().Contain(x => x.Key == KeyBuilder.BuildKey<string>("d"));
        _cacheMonitor.GetInfos().SelectMany(x => x.Items).Sum(x => x.Value.Size).Should().BeGreaterThan(0);
    }
}