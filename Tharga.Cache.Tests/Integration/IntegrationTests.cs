using FluentAssertions;
using System.Net.Http.Json;
using Tharga.Cache.Web;
using Tharga.Cache.Web.Controllers;
using Xunit;

namespace Tharga.Cache.Tests.Integration;

[Trait("Category", "Integration")]
public class IntegrationTests : TestFixtureBase
{
    private readonly string _controller = "MemoryCache";

    public IntegrationTests(CustomWebApplicationFactory<Program> factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Get()
    {
        //Arrange
        var key = Guid.NewGuid().ToString();
        var first = await _client.GetFromJsonAsync<MemoryData>($"{_controller}/{key}");

        //Act
        var second = await _client.GetFromJsonAsync<MemoryData>($"{_controller}/{key}");

        //Assert
        first.Guid.Should().Be(second.Guid);
    }

    [Fact]
    public async Task MonitorGet()
    {
        //Arrange
        var key = Guid.NewGuid().ToString();
        await _client.GetFromJsonAsync<MemoryData>($"{_controller}/{key}");

        //Act
        var monitor = await _client.GetFromJsonAsync<MonitorData>("monitor");

        //Assert
        monitor.Infos.Select(x => x.Key).Any(x => x.EndsWith(key)).Should().BeTrue();
        monitor.Infos.Single(x => x.Key.EndsWith(key)).AccessCount.Should().Be(1);
    }

    [Fact]
    public async Task MonitorDrop()
    {
        //Arrange
        var key = Guid.NewGuid().ToString();
        await _client.GetFromJsonAsync<MemoryData>($"{_controller}/{key}");
        await _client.DeleteAsync($"{_controller}/{key}");

        //Act
        var monitor = await _client.GetFromJsonAsync<MonitorData>("monitor");

        //Assert
        monitor.Infos.Select(x => x.Key).Any(x => x.EndsWith(key)).Should().BeFalse();
    }
}