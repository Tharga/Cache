using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Tharga.Cache.Tests;

public class FirstCallTests
{
    private int _dataSetEventCount;
    private int _dataGetEventCount;
    private int _dataDropEventCount;

    [Theory]
    [ClassData(typeof(AllTypes))]
    public async Task GetValueAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var value = "value";
        var sut = CacheTypeLoader.GetCache(type, evictionPolicy, staleWhileRevalidate);
        sut.DataSetEvent += (_, _) => { _dataSetEventCount++; };
        sut.DataGetEvent += (_, _) => { _dataGetEventCount++; };
        sut.DataDropEvent += (_, _) => { _dataDropEventCount++; };

        //Act
        var item = await sut.GetAsync("Key", () => Task.FromResult(value));

        //Assert
        _dataSetEventCount.Should().Be(1);
        _dataGetEventCount.Should().Be(1);
        _dataDropEventCount.Should().Be(0);
        item.Should().Be(value);
    }

    [Theory]
    [ClassData(typeof(AllTypes))]
    public async Task PeekValueAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var sut = CacheTypeLoader.GetCache(type, evictionPolicy, staleWhileRevalidate);
        sut.DataSetEvent += (_, _) => { _dataSetEventCount++; };
        sut.DataGetEvent += (_, _) => { _dataGetEventCount++; };
        sut.DataDropEvent += (_, _) => { _dataDropEventCount++; };

        //Act
        var item = await sut.PeekAsync<string>("Key");

        //Assert
        _dataSetEventCount.Should().Be(0);
        _dataGetEventCount.Should().Be(0);
        _dataDropEventCount.Should().Be(0);
        item.Should().BeNull();
    }

    [Theory]
    [ClassData(typeof(AllTypes))]
    public async Task SetValueAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var value = "value";
        var sut = CacheTypeLoader.GetCache(type, evictionPolicy, staleWhileRevalidate);
        sut.DataSetEvent += (_, _) => { _dataSetEventCount++; };
        sut.DataGetEvent += (_, _) => { _dataGetEventCount++; };
        sut.DataDropEvent += (_, _) => { _dataDropEventCount++; };

        //Act
        await sut.SetAsync("Key", value);

        //Assert
        _dataSetEventCount.Should().Be(1);
        _dataGetEventCount.Should().Be(0);
        _dataDropEventCount.Should().Be(0);
        var item = await sut.GetAsync("Key", () => Task.FromResult("crap"));
        item.Should().Be(value);
    }

    [Theory]
    [ClassData(typeof(AllTypes))]
    public async Task DropValueAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var sut = CacheTypeLoader.GetCache(type, evictionPolicy, staleWhileRevalidate);
        sut.DataSetEvent += (_, _) => { _dataSetEventCount++; };
        sut.DataGetEvent += (_, _) => { _dataGetEventCount++; };
        sut.DataDropEvent += (_, _) => { _dataDropEventCount++; };

        //Act
        var item = await sut.DropAsync<string>("Key");

        //Assert
        _dataSetEventCount.Should().Be(0);
        _dataGetEventCount.Should().Be(0);
        _dataDropEventCount.Should().Be(0);
        item.Should().BeNull();
    }
}