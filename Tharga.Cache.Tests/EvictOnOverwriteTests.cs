using FluentAssertions;
using Moq;
using Tharga.Cache.Core;
using Tharga.Cache.Persist;
using Xunit;

namespace Tharga.Cache.Tests;

public class EvictOnOverwriteTests
{
    private readonly Mock<IPersistLoader> _persistLoader = new(MockBehavior.Strict);
    private readonly IFetchQueue _fetchQueue;
    private readonly CacheMonitor _cacheMonitor;

    public EvictOnOverwriteTests()
    {
        var options = new CacheOptions();
        _cacheMonitor = new CacheMonitor(_persistLoader.Object, options);
        _persistLoader.Setup(x => x.GetPersist(It.IsAny<Type>())).Returns(new Memory(_cacheMonitor));
        _fetchQueue = new FetchQueue(_cacheMonitor, options, null);
    }

    [Theory]
    [InlineData(EvictionPolicy.FirstInFirstOut)]
    [InlineData(EvictionPolicy.LeastRecentlyUsed)]
    [InlineData(EvictionPolicy.RandomReplacement)]
    public async Task MaxCount_one_keeps_the_value_written_last(EvictionPolicy policy)
    {
        //Arrange
        var sut = Build(o => { o.MaxCount = 1; o.EvictionPolicy = policy; });
        await sut.SetAsync("k", "v1");

        //Act
        await sut.SetAsync("k", "v2");

        //Assert
        (await sut.PeekAsync<string>("k")).Should().Be("v2");
        _cacheMonitor.GetInfos().Single().Items.Keys.Should().BeEquivalentTo(["String.k"]);
    }

    [Theory]
    [InlineData(EvictionPolicy.FirstInFirstOut)]
    [InlineData(EvictionPolicy.LeastRecentlyUsed)]
    [InlineData(EvictionPolicy.RandomReplacement)]
    public async Task Overwriting_an_existing_key_at_capacity_evicts_nothing(EvictionPolicy policy)
    {
        //Arrange
        var drops = 0;
        var sut = Build(o => { o.MaxCount = 3; o.EvictionPolicy = policy; });
        sut.DataDropEvent += (_, _) => drops++;
        await sut.SetAsync("a", "aa");
        await sut.SetAsync("b", "bb");
        await sut.SetAsync("c", "cc");
        await sut.PeekAsync<string>("a");
        await sut.PeekAsync<string>("b");

        //Act
        await sut.SetAsync("c", "cc2");

        //Assert
        drops.Should().Be(0);
        (await sut.PeekAsync<string>("a")).Should().Be("aa");
        (await sut.PeekAsync<string>("b")).Should().Be("bb");
        (await sut.PeekAsync<string>("c")).Should().Be("cc2");
        _cacheMonitor.GetInfos().Single().Items.Count.Should().Be(3);
    }

    [Fact]
    public async Task A_new_key_at_capacity_still_evicts_the_chosen_item()
    {
        //Arrange
        var drops = 0;
        var sut = Build(o => { o.MaxCount = 3; o.EvictionPolicy = EvictionPolicy.FirstInFirstOut; });
        sut.DataDropEvent += (_, _) => drops++;
        await sut.SetAsync("a", "aa");
        await sut.SetAsync("b", "bb");
        await sut.SetAsync("c", "cc");
        await sut.SetAsync("b", "bb2"); //NOTE: Overwrite, must not evict.

        //Act
        await sut.SetAsync("d", "dd");

        //Assert
        drops.Should().Be(1);
        (await sut.PeekAsync<string>("a")).Should().BeNull();
        (await sut.PeekAsync<string>("b")).Should().Be("bb2");
        (await sut.PeekAsync<string>("c")).Should().Be("cc");
        (await sut.PeekAsync<string>("d")).Should().Be("dd");
    }

    [Fact]
    public async Task MaxSize_overwrite_that_fits_evicts_nothing()
    {
        //Arrange
        var drops = 0;
        var sut = Build(o => { o.MaxSize = 13; o.EvictionPolicy = EvictionPolicy.FirstInFirstOut; });
        sut.DataDropEvent += (_, _) => drops++;
        await sut.SetAsync("a", "aa");
        await sut.SetAsync("b", "bb");
        await sut.SetAsync("c", "cc");

        //Act
        await sut.SetAsync("a", "xx"); //NOTE: Same size as before, the total does not grow.

        //Assert
        drops.Should().Be(0);
        (await sut.PeekAsync<string>("a")).Should().Be("xx");
        (await sut.PeekAsync<string>("b")).Should().Be("bb");
        (await sut.PeekAsync<string>("c")).Should().Be("cc");
    }

    [Fact]
    public async Task MaxSize_overwrite_that_overflows_evicts_another_item_never_the_written_one()
    {
        //Arrange
        var sut = Build(o => { o.MaxSize = 13; o.EvictionPolicy = EvictionPolicy.FirstInFirstOut; });
        await sut.SetAsync("a", "aa");
        await sut.SetAsync("b", "bb");
        await sut.SetAsync("c", "cc");

        //Act
        await sut.SetAsync("a", "aaaaaaa"); //NOTE: "a" is the oldest, so FirstInFirstOut would pick it.

        //Assert
        (await sut.PeekAsync<string>("a")).Should().Be("aaaaaaa");
        (await sut.PeekAsync<string>("b")).Should().BeNull();
        (await sut.PeekAsync<string>("c")).Should().Be("cc");
    }

    private EternalCache Build(Action<CacheTypeOptions> configure)
    {
        var options = new CacheOptions();
        options.RegisterType<string, IMemory>(configure);
        return new EternalCache(_cacheMonitor, _persistLoader.Object, _fetchQueue, options);
    }
}
