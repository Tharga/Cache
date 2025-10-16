using FluentAssertions;
using System.Net.Http.Json;
using Tharga.Cache.Web;
using Xunit;

namespace Tharga.Cache.MongoDB.Tests;

[Trait("Category", "Integration")]
public class IntegrationTests : TestFixtureBase
{
    private readonly string _controller = "MongoDBCache";

    public IntegrationTests(CustomWebApplicationFactory<Program> factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Get()
    {
        //Arrange
        var key = Guid.NewGuid().ToString();
        var first = await _client.GetFromJsonAsync<MongoDBData>($"{_controller}/{key}");

        //Act
        var second = await _client.GetFromJsonAsync<MongoDBData>($"{_controller}/{key}");

        //Assert
        first.Guid.Should().Be(second.Guid);
    }
}