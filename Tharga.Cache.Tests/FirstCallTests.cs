using System.Diagnostics;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Tharga.Cache.Tests;

public class FirstCallTests
{
    [Theory]
    [ClassData(typeof(AllTypes))]
    public async Task GetValueAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var value = "value";
        var dataSetEventCount = 0;
        var sut = CacheTypeLoader.GetCache(type, evictionPolicy, staleWhileRevalidate);
        sut.DataSetEvent += (_, _) => { dataSetEventCount++; };

        //Act
        var item = await sut.GetAsync("Key", () => Task.FromResult(value));

        //Assert
        item.Should().Be(value);
        dataSetEventCount.Should().Be(1);
    }

    [Theory]
    [ClassData(typeof(AllTypes))]
    public async Task PeekValueAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var sut = CacheTypeLoader.GetCache(type, evictionPolicy, staleWhileRevalidate);

        //Act
        var item = await sut.PeekAsync<string>("Key");

        //Assert
        item.Should().BeNull();
    }

    [Theory]
    [ClassData(typeof(AllTypes))]
    public async Task SetValueAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var value = "value";
        var dataSetEventCount = 0;
        var sut = CacheTypeLoader.GetCache(type, evictionPolicy, staleWhileRevalidate);
        sut.DataSetEvent += (_, _) => { dataSetEventCount++; };

        //Act
        await sut.SetAsync("Key", value);

        //Assert
        var item = await sut.GetAsync("Key", () => Task.FromResult("crap"));
        item.Should().Be(value);
        dataSetEventCount.Should().Be(1);
    }

    [Theory]
    [ClassData(typeof(AllTypes))]
    public async Task DropValueAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var sut = CacheTypeLoader.GetCache(type, evictionPolicy, staleWhileRevalidate);

        //Act
        await sut.DropAsync<string>("Key");

        //Assert
    }
}