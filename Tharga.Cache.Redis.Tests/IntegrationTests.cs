using System.Net.Http.Json;
using FluentAssertions;
using Tharga.Cache.Web;
using Xunit;

namespace Tharga.Cache.Redis.Tests;

[Trait("Category", "Integration")]
public class IntegrationTests : TestFixtureBase
{
    private readonly string _controller = "RedisCache";

    public IntegrationTests(CustomWebApplicationFactory<MemoryData> factory)
        : base(factory)
    {
    }

    [Fact(Skip = "Set up redis")]
    public async Task Get()
    {
        //Arrange
        var key = Guid.NewGuid().ToString();
        var first = await _client.GetFromJsonAsync<RedisData>($"{_controller}/{key}");

        //Act
        var second = await _client.GetFromJsonAsync<RedisData>($"{_controller}/{key}");

        //Assert
        first.Guid.Should().Be(second.Guid);
    }
}