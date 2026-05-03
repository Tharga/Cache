using System.Text.Json;
using FluentAssertions;
using Moq;
using Tharga.Cache.Mcp;
using Xunit;

namespace Tharga.Cache.Tests;

public class CacheToolProviderTests
{
    [Fact]
    public async Task ListTools_returns_clear_stale_and_clear_all()
    {
        //Arrange
        var monitor = Mock.Of<ICacheMonitor>();
        var sut = new CacheToolProvider(monitor);

        //Act
        var tools = await sut.ListToolsAsync(null, default);

        //Assert
        tools.Select(t => t.Name).Should().BeEquivalentTo("cache.clear_stale", "cache.clear_all");
    }

    [Fact]
    public async Task Call_clear_stale_invokes_monitor_ClearStale()
    {
        //Arrange
        var monitor = new Mock<ICacheMonitor>();
        var sut = new CacheToolProvider(monitor.Object);

        //Act
        var result = await sut.CallToolAsync("cache.clear_stale", default, null, default);

        //Assert
        result.IsError.Should().BeFalse();
        monitor.Verify(x => x.ClearStale(), Times.Once);
        monitor.Verify(x => x.ClearAll(), Times.Never);
    }

    [Fact]
    public async Task Call_clear_all_invokes_monitor_ClearAll()
    {
        //Arrange
        var monitor = new Mock<ICacheMonitor>();
        var sut = new CacheToolProvider(monitor.Object);

        //Act
        var result = await sut.CallToolAsync("cache.clear_all", default, null, default);

        //Assert
        result.IsError.Should().BeFalse();
        monitor.Verify(x => x.ClearAll(), Times.Once);
        monitor.Verify(x => x.ClearStale(), Times.Never);
    }

    [Fact]
    public async Task Call_unknown_tool_returns_error_result()
    {
        //Arrange
        var monitor = Mock.Of<ICacheMonitor>();
        var sut = new CacheToolProvider(monitor);

        //Act
        var result = await sut.CallToolAsync("cache.does_not_exist", default, null, default);

        //Assert
        result.IsError.Should().BeTrue();
        result.Content.Single().Text.Should().Contain("Unknown tool");
    }
}
