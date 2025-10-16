using FluentAssertions;
using Moq;
using Tharga.Cache.Core;
using Tharga.Cache.Persist;
using Xunit;

namespace Tharga.Cache.Tests;

public class UnloadDataTests
{
    private readonly Mock<IPersistLoader> _persistLoader = new(MockBehavior.Strict);
    private readonly IFetchQueue _fetchQueue;
    private readonly CacheMonitor _cacheMonitor;

    public UnloadDataTests()
    {
        var options = new CacheOptions();
        _cacheMonitor = new CacheMonitor(_persistLoader.Object, options);
        _persistLoader.Setup(x => x.GetPersist(It.IsAny<Type>())).Returns(new Memory(_cacheMonitor));
        _fetchQueue = new FetchQueue(_cacheMonitor, options, null);
    }

    [Fact]
    public async Task FirstInFirstOut_MaxCount()
    {
        //Arrange
        var dataDropEventCount = 0;
        var monitorSetEventCount = 0;
        var monitorGetEventCount = 0;
        var monitorDropEventCount = 0;
        var options = new CacheOptions();
        options.RegisterType<string, IMemory>(o =>
        {
            o.MaxCount = 3;
            o.EvictionPolicy = EvictionPolicy.FirstInFirstOut;
        });
        var sut = new EternalCache(_cacheMonitor, _persistLoader.Object, _fetchQueue, options);
        sut.DataDropEvent += (_, _) => dataDropEventCount++;
        _cacheMonitor.DataSetEvent += (_, _) => { monitorSetEventCount++; };
        _cacheMonitor.DataGetEvent += (_, _) => { monitorGetEventCount++; };
        _cacheMonitor.DataDropEvent += (_, _) => { monitorDropEventCount++; };
        await sut.SetAsync("a", "aa");
        await sut.SetAsync("b", "bb");
        await sut.SetAsync("c", "cc");

        //Act
        await sut.SetAsync("d", "dd");

        //Assert
        dataDropEventCount.Should().Be(1);
        monitorSetEventCount.Should().Be(4);
        monitorGetEventCount.Should().Be(0);
        monitorDropEventCount.Should().Be(1);
        _cacheMonitor.GetInfos().Single().Items.Count.Should().Be(3);
        _cacheMonitor.GetInfos().Single().Items.Should().Contain(x => x.Key == KeyBuilder.SetTypeKey<string>("d"));
        _cacheMonitor.GetInfos().Single().Items.Should().NotContain(x => x.Key == KeyBuilder.SetTypeKey<string>("a"));
        _cacheMonitor.GetInfos().SelectMany(x => x.Items).Sum(x => x.Value.Size).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task FirstInFirstOut_LeastRecentlyUsed()
    {
        //Arrange
        var dataDropEventCount = 0;
        var monitorSetEventCount = 0;
        var monitorGetEventCount = 0;
        var monitorDropEventCount = 0;
        var options = new CacheOptions();
        options.RegisterType<string, IMemory>(o =>
        {
            o.MaxCount = 3;
            o.EvictionPolicy = EvictionPolicy.LeastRecentlyUsed;
        });
        var sut = new EternalCache(_cacheMonitor, _persistLoader.Object, _fetchQueue, options);
        sut.DataDropEvent += (_, _) => dataDropEventCount++;
        _cacheMonitor.DataSetEvent += (_, _) => { monitorSetEventCount++; };
        _cacheMonitor.DataGetEvent += (_, _) => { monitorGetEventCount++; };
        _cacheMonitor.DataDropEvent += (_, _) => { monitorDropEventCount++; };
        await sut.GetAsync("a", () => Task.FromResult("aa"));
        await sut.GetAsync("b", () => Task.FromResult("bb"));
        await sut.GetAsync("c", () => Task.FromResult("cc"));
        await sut.PeekAsync<string>("a"); //NOTE: Access "a", so it should not be evicted.

        //Act
        await sut.SetAsync("d", "dd");

        //Assert
        dataDropEventCount.Should().Be(1);
        monitorSetEventCount.Should().Be(4);
        monitorGetEventCount.Should().Be(4);
        monitorDropEventCount.Should().Be(1);
        _cacheMonitor.GetInfos().Single().Items.Count.Should().Be(3);
        _cacheMonitor.GetInfos().Single().Items.Should().Contain(x => x.Key == KeyBuilder.SetTypeKey<string>("b"));
        _cacheMonitor.GetInfos().Single().Items.Should().NotContain(x => x.Key == KeyBuilder.SetTypeKey<string>("a"));
        _cacheMonitor.GetInfos().SelectMany(x => x.Items).Sum(x => x.Value.Size).Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task FirstInFirstOut_RandomReplacement()
    {
        //Arrange
        var dataDropEventCount = 0;
        var monitorSetEventCount = 0;
        var monitorGetEventCount = 0;
        var monitorDropEventCount = 0;
        var options = new CacheOptions();
        options.RegisterType<string, IMemory>(o =>
        {
            o.MaxCount = 3;
            o.EvictionPolicy = EvictionPolicy.RandomReplacement;
        });
        var sut = new EternalCache(_cacheMonitor, _persistLoader.Object, _fetchQueue, options);
        sut.DataDropEvent += (_, _) => dataDropEventCount++;
        _cacheMonitor.DataSetEvent += (_, _) => { monitorSetEventCount++; };
        _cacheMonitor.DataGetEvent += (_, _) => { monitorGetEventCount++; };
        _cacheMonitor.DataDropEvent += (_, _) => { monitorDropEventCount++; };
        await sut.SetAsync("a", "aa");
        await sut.SetAsync("b", "bb");
        await sut.SetAsync("c", "cc");

        //Act
        await sut.SetAsync("d", "dd");

        //Assert
        dataDropEventCount.Should().Be(1);
        monitorSetEventCount.Should().Be(4);
        monitorGetEventCount.Should().Be(0);
        monitorDropEventCount.Should().Be(1);
        _cacheMonitor.GetInfos().Single().Items.Count.Should().Be(3);
        _cacheMonitor.GetInfos().Single().Items.Should().Contain(x => x.Key == KeyBuilder.SetTypeKey<string>("d"));
        _cacheMonitor.GetInfos().SelectMany(x => x.Items).Sum(x => x.Value.Size).Should().BeGreaterThan(0);
    }
}