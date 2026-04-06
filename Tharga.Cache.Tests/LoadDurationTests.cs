using FluentAssertions;
using Moq;
using Tharga.Cache.Core;
using Tharga.Cache.Persist;
using Xunit;

namespace Tharga.Cache.Tests;

public class LoadDurationTests
{
    [Fact]
    public async Task GetAsync_FirstCall_LoadDurationIsRecorded()
    {
        //Arrange
        var options = new CacheOptions
        {
            Default = new CacheTypeOptions { DefaultFreshSpan = TimeSpan.FromSeconds(30) }
        };
        options.RegisterType<string, IMemory>(s => s.DefaultFreshSpan = TimeSpan.FromSeconds(30));

        var persistLoader = new Mock<IPersistLoader>(MockBehavior.Strict);
        var cacheMonitor = new CacheMonitor(persistLoader.Object, options);
        var memory = new Memory(cacheMonitor);
        persistLoader.Setup(x => x.GetPersist(It.IsAny<Type>())).Returns(memory);
        var fetchQueue = new FetchQueue(cacheMonitor, options, null);
        var cache = new TimeToLiveCache(cacheMonitor, persistLoader.Object, fetchQueue, options);

        //Act
        await cache.GetAsync<string>("Key", async () =>
        {
            await Task.Delay(50);
            return "value";
        });

        //Assert
        var items = cacheMonitor.GetInfos().SelectMany(x => x.Items).ToArray();
        items.Should().HaveCount(1);
        items[0].Value.LoadDuration.Should().NotBeNull();
        items[0].Value.LoadDuration.Value.TotalMilliseconds.Should().BeGreaterThanOrEqualTo(30);
    }

    [Fact]
    public async Task GetAsync_FromPersistence_LoadDurationIsNull()
    {
        //Arrange
        var options = new CacheOptions
        {
            Default = new CacheTypeOptions { DefaultFreshSpan = TimeSpan.FromSeconds(30) }
        };
        options.RegisterType<string, IMemory>(s => s.DefaultFreshSpan = TimeSpan.FromSeconds(30));

        var persistLoader = new Mock<IPersistLoader>(MockBehavior.Strict);
        var cacheMonitor = new CacheMonitor(persistLoader.Object, options);
        var memory = new Memory(cacheMonitor);
        persistLoader.Setup(x => x.GetPersist(It.IsAny<Type>())).Returns(memory);
        var fetchQueue = new FetchQueue(cacheMonitor, options, null);
        var cache = new TimeToLiveCache(cacheMonitor, persistLoader.Object, fetchQueue, options);

        // Pre-populate persistence without LoadDuration (simulating restart)
        var item = CacheItemBuilder.BuildCacheItem(new Dictionary<string, string>(), "pre-existing", TimeSpan.FromSeconds(30));
        var key = ((Key)"PrePopulated").SetTypeKey<string>();
        await memory.SetAsync(key, item, false);

        //Act
        await cache.GetAsync<string>("PrePopulated", () => Task.FromResult("fallback"));

        //Assert
        var items = cacheMonitor.GetInfos().SelectMany(x => x.Items).ToArray();
        items.Should().HaveCount(1);
        items[0].Value.LoadDuration.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_SecondCall_LoadDurationPreserved()
    {
        //Arrange
        var options = new CacheOptions
        {
            Default = new CacheTypeOptions { DefaultFreshSpan = TimeSpan.FromSeconds(30) }
        };
        options.RegisterType<string, IMemory>(s => s.DefaultFreshSpan = TimeSpan.FromSeconds(30));

        var persistLoader = new Mock<IPersistLoader>(MockBehavior.Strict);
        var cacheMonitor = new CacheMonitor(persistLoader.Object, options);
        var memory = new Memory(cacheMonitor);
        persistLoader.Setup(x => x.GetPersist(It.IsAny<Type>())).Returns(memory);
        var fetchQueue = new FetchQueue(cacheMonitor, options, null);
        var cache = new TimeToLiveCache(cacheMonitor, persistLoader.Object, fetchQueue, options);

        //Act
        await cache.GetAsync<string>("Key", async () =>
        {
            await Task.Delay(50);
            return "value";
        });

        // Second call — cache hit, no fetch
        await cache.GetAsync<string>("Key", () => Task.FromResult("should-not-be-called"));

        //Assert
        var items = cacheMonitor.GetInfos().SelectMany(x => x.Items).ToArray();
        items.Should().HaveCount(1);
        items[0].Value.LoadDuration.Should().NotBeNull();
        items[0].Value.LoadDuration.Value.TotalMilliseconds.Should().BeGreaterThanOrEqualTo(30);
    }
}
