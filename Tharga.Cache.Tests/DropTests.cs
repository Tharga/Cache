using FluentAssertions;
using Tharga.Cache.Tests.Helper;
using Xunit;

namespace Tharga.Cache.Tests;

public class DropTests
{
    [Theory]
    [ClassData(typeof(AllTypes))]
    public async Task DropAllByTypeAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var result = CacheTypeLoader.GetCache(type, evictionPolicy, staleWhileRevalidate);
        var sut = result.Cache;
        await sut.SetAsync("Key1", "a");
        await sut.SetAsync("Key2", "b");
        await sut.SetAsync("Key3", 1);

        //Act
        var item = await sut.DropAsync<string>(KeyBuilder.Empty);

        //Assert
        item.Should().Be(2);
        result.Monitor.GetInfos().SelectMany(x => x.Items).Sum(x => x.Value.Size).Should().Be(1);
        sut.PeekAsync<string>("Key1").Should().NotBeNull();
        sut.PeekAsync<string>("Key2").Should().NotBeNull();
        sut.PeekAsync<int>("Key3").Should().NotBeNull();
    }

    [Theory]
    [ClassData(typeof(AllTypes))]
    public async Task DropByKeyPartTypeAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var result = CacheTypeLoader.GetCache(type, evictionPolicy, staleWhileRevalidate);
        var sut = result.Cache;
        await sut.SetAsync(KeyBuilder.Set("A", "a1").Set("B", "b1"), "a");
        await sut.SetAsync(KeyBuilder.Set("A", "a1").Set("B", "b2"), "b");
        await sut.SetAsync(KeyBuilder.Set("A", "a2").Set("B", "b3"), "c");

        //Act
        var item = await sut.DropAsync<string>(KeyBuilder.Set("A", "a1"));

        //Assert
        item.Should().Be(2);
        result.Monitor.GetInfos().SelectMany(x => x.Items).Count().Should().Be(1);
    }
}