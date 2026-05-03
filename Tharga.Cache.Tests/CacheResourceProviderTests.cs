using System.Text.Json;
using FluentAssertions;
using Moq;
using Tharga.Cache.Core;
using Tharga.Cache.Mcp;
using Tharga.Cache.Persist;
using Xunit;

namespace Tharga.Cache.Tests;

public class CacheResourceProviderTests
{
    [Fact]
    public async Task ListResources_returns_all_four_descriptors()
    {
        //Arrange
        var monitor = Mock.Of<ICacheMonitor>();
        var sut = new CacheResourceProvider(monitor);

        //Act
        var resources = await sut.ListResourcesAsync(null, default);

        //Assert
        resources.Should().HaveCount(4);
        resources.Select(r => r.Uri).Should().BeEquivalentTo(
            "cache://types",
            "cache://items",
            "cache://health",
            "cache://queue");
    }

    [Fact]
    public async Task Read_types_includes_registered_types()
    {
        //Arrange
        var (monitor, _) = BuildPopulatedMonitor();
        var sut = new CacheResourceProvider(monitor);

        //Act
        var content = await sut.ReadResourceAsync("cache://types", null, default);

        //Assert
        content.Uri.Should().Be("cache://types");
        content.MimeType.Should().Be("application/json");
        var json = JsonDocument.Parse(content.Text);
        var types = json.RootElement.GetProperty("types");
        types.GetArrayLength().Should().Be(1);
        types[0].GetProperty("type").GetString().Should().Contain("String");
        types[0].GetProperty("persistType").GetString().Should().Be("Memory");
        types[0].GetProperty("count").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task Read_items_returns_individual_entries()
    {
        //Arrange
        var (monitor, _) = BuildPopulatedMonitor();
        var sut = new CacheResourceProvider(monitor);

        //Act
        var content = await sut.ReadResourceAsync("cache://items", null, default);

        //Assert
        var json = JsonDocument.Parse(content.Text);
        var items = json.RootElement.GetProperty("items");
        items.GetArrayLength().Should().Be(1);
        items[0].GetProperty("key").GetString().Should().Contain("ItemKey");
        items[0].GetProperty("persistType").GetString().Should().Be("Memory");
    }

    [Fact]
    public async Task Read_health_returns_backend_status()
    {
        //Arrange
        var (monitor, _) = BuildPopulatedMonitor();
        var sut = new CacheResourceProvider(monitor);

        //Act
        var content = await sut.ReadResourceAsync("cache://health", null, default);

        //Assert
        var json = JsonDocument.Parse(content.Text);
        var health = json.RootElement.GetProperty("health");
        health.GetArrayLength().Should().Be(1);
        health[0].GetProperty("type").GetString().Should().Be("IMemory");
        health[0].GetProperty("success").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public async Task Read_queue_returns_queue_depth()
    {
        //Arrange
        var (monitor, _) = BuildPopulatedMonitor();
        var sut = new CacheResourceProvider(monitor);

        //Act
        var content = await sut.ReadResourceAsync("cache://queue", null, default);

        //Assert
        var json = JsonDocument.Parse(content.Text);
        json.RootElement.GetProperty("queueDepth").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task Read_unknown_uri_returns_explanatory_text()
    {
        //Arrange
        var monitor = Mock.Of<ICacheMonitor>();
        var sut = new CacheResourceProvider(monitor);

        //Act
        var content = await sut.ReadResourceAsync("cache://unknown", null, default);

        //Assert
        content.Text.Should().Contain("Unknown resource");
    }

    private static (ICacheMonitor Monitor, ICache Cache) BuildPopulatedMonitor()
    {
        // Build a real CacheMonitor with one item so the resource payloads have substance.
        var options = new CacheOptions
        {
            Default = new CacheTypeOptions { DefaultFreshSpan = TimeSpan.FromMinutes(5) },
        };
        options.RegisterType<string, IMemory>(s => s.DefaultFreshSpan = TimeSpan.FromMinutes(5));

        var persistLoader = new Mock<IPersistLoader>(MockBehavior.Strict);
        var monitor = new Tharga.Cache.Core.CacheMonitor(persistLoader.Object, options);
        var memory = new Memory(monitor);
        persistLoader.Setup(x => x.GetPersist(It.IsAny<Type>())).Returns(memory);
        var fetchQueue = new Tharga.Cache.Core.FetchQueue(monitor, options, null);
        var cache = new Tharga.Cache.Core.TimeToLiveCache(monitor, persistLoader.Object, fetchQueue, options);

        // Populate with one item.
        cache.GetAsync<string>("ItemKey", () => Task.FromResult("value")).GetAwaiter().GetResult();

        return (monitor, cache);
    }
}
