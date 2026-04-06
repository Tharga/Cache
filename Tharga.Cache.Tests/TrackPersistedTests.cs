using FluentAssertions;
using Moq;
using Tharga.Cache.Core;
using Tharga.Cache.Persist;
using Xunit;

namespace Tharga.Cache.Tests;

public class TrackPersistedTests
{
    [Fact]
    public async Task GetAsync_FreshItemInPersistence_AppearsInMonitor()
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

        // Pre-populate persistence (simulating data surviving a restart)
        var item = CacheItemBuilder.BuildCacheItem(new Dictionary<string, string>(), "pre-existing", TimeSpan.FromSeconds(30));
        var key = ((Key)"PrePopulated").SetTypeKey<string>();
        await memory.SetAsync(key, item, false);

        // Verify monitor is empty before access
        cacheMonitor.GetInfos().SelectMany(x => x.Items).Should().BeEmpty();

        //Act
        var result = await cache.GetAsync<string>("PrePopulated", () => Task.FromResult("should-not-be-called"));

        //Assert
        result.Should().Be("pre-existing");
        cacheMonitor.GetInfos().SelectMany(x => x.Items).Should().HaveCount(1);
        cacheMonitor.GetInfos().SelectMany(x => x.Items).First().Value.Size.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAsync_FreshItemInPersistence_DoesNotFireDataSetEvent()
    {
        //Arrange
        var dataSetEventCount = 0;
        var dataGetEventCount = 0;

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

        cache.DataSetEvent += (_, _) => dataSetEventCount++;
        cache.DataGetEvent += (_, _) => dataGetEventCount++;

        // Pre-populate persistence
        var item = CacheItemBuilder.BuildCacheItem(new Dictionary<string, string>(), "pre-existing", TimeSpan.FromSeconds(30));
        var key = ((Key)"PrePopulated").SetTypeKey<string>();
        await memory.SetAsync(key, item, false);

        //Act
        await cache.GetAsync<string>("PrePopulated", () => Task.FromResult("should-not-be-called"));

        //Assert
        dataSetEventCount.Should().Be(0, "Track should not fire DataSetEvent");
        dataGetEventCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAsync_FreshItemInPersistence_TrackIsIdempotent()
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

        // Pre-populate persistence
        var item = CacheItemBuilder.BuildCacheItem(new Dictionary<string, string>(), "pre-existing", TimeSpan.FromSeconds(30));
        var key = ((Key)"PrePopulated").SetTypeKey<string>();
        await memory.SetAsync(key, item, false);

        //Act — access twice
        await cache.GetAsync<string>("PrePopulated", () => Task.FromResult("fallback"));
        await cache.GetAsync<string>("PrePopulated", () => Task.FromResult("fallback"));

        //Assert — still only one item tracked
        cacheMonitor.GetInfos().SelectMany(x => x.Items).Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAsync_FreshItemInPersistence_MonitorSetEventNotFired()
    {
        //Arrange
        var monitorSetEventCount = 0;

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

        cacheMonitor.DataSetEvent += (_, _) => monitorSetEventCount++;

        // Pre-populate persistence
        var item = CacheItemBuilder.BuildCacheItem(new Dictionary<string, string>(), "pre-existing", TimeSpan.FromSeconds(30));
        var key = ((Key)"PrePopulated").SetTypeKey<string>();
        await memory.SetAsync(key, item, false);

        //Act
        await cache.GetAsync<string>("PrePopulated", () => Task.FromResult("fallback"));

        //Assert
        monitorSetEventCount.Should().Be(0, "Track should not fire monitor DataSetEvent");
    }
}
