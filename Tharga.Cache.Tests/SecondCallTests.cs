using FluentAssertions;
using Xunit;

namespace Tharga.Cache.Tests;

public class SecondCallTests
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
        await sut.SetAsync("Key", value);
        sut.DataSetEvent += (_, _) => { _dataSetEventCount++; };
        sut.DataGetEvent += (_, _) => { _dataGetEventCount++; };
        sut.DataDropEvent += (_, _) => { _dataDropEventCount++; };

        //Act
        var item = await sut.GetAsync("Key", () => Task.FromResult(value));

        //Assert
        _dataSetEventCount.Should().Be(0);
        _dataGetEventCount.Should().Be(1);
        _dataDropEventCount.Should().Be(0);
        item.Should().Be(value);
    }

    [Theory]
    [ClassData(typeof(AllTypes))]
    public async Task PeekValueAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var value = "value";
        var sut = CacheTypeLoader.GetCache(type, evictionPolicy, staleWhileRevalidate);
        await sut.SetAsync("Key", value);
        sut.DataSetEvent += (_, _) => { _dataSetEventCount++; };
        sut.DataGetEvent += (_, _) => { _dataGetEventCount++; };
        sut.DataDropEvent += (_, _) => { _dataDropEventCount++; };

        //Act
        var item = await sut.PeekAsync<string>("Key");

        //Assert
        _dataSetEventCount.Should().Be(0);
        _dataGetEventCount.Should().Be(1);
        _dataDropEventCount.Should().Be(0);
        item.Should().Be(value);
    }

    [Theory]
    [ClassData(typeof(AllTypes))]
    public async Task SetValueAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var value = "value";
        var sut = CacheTypeLoader.GetCache(type, evictionPolicy, staleWhileRevalidate);
        await sut.SetAsync("Key", value);
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
        var value = "value";
        var sut = CacheTypeLoader.GetCache(type, evictionPolicy, staleWhileRevalidate);
        await sut.SetAsync("Key", value);
        sut.DataSetEvent += (_, _) => { _dataSetEventCount++; };
        sut.DataGetEvent += (_, _) => { _dataGetEventCount++; };
        sut.DataDropEvent += (_, _) => { _dataDropEventCount++; };

        //Act
        var item = await sut.DropAsync<string>("Key");

        //Assert
        _dataSetEventCount.Should().Be(0);
        _dataGetEventCount.Should().Be(0);
        _dataDropEventCount.Should().Be(1);
        item.Should().Be(value);
    }
}

public class ExpiredEndingCacheTests
{
    private int _dataSetEventCount;
    private int _dataGetEventCount;
    private int _dataDropEventCount;

    [Theory]
    [ClassData(typeof(EndingCacheTypes))]
    public async Task GetValueAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var value = "value";
        var sut = CacheTypeLoader.GetCache<ITimeCache>(type, evictionPolicy, staleWhileRevalidate);
        await sut.SetAsync("Key", value, TimeSpan.FromSeconds(1));
        sut.DataSetEvent += (_, _) => { _dataSetEventCount++; };
        sut.DataGetEvent += (_, _) => { _dataGetEventCount++; };
        sut.DataDropEvent += (_, _) => { _dataDropEventCount++; };

        //Act
        var item = await sut.GetAsync("Key", () => Task.FromResult(value));

        //Assert
        _dataSetEventCount.Should().Be(0);
        _dataGetEventCount.Should().Be(1);
        _dataDropEventCount.Should().Be(0);
        item.Should().Be(value);
    }

    [Theory]
    [ClassData(typeof(EndingCacheTypes))]
    public async Task PeekValueAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var value = "value";
        var sut = CacheTypeLoader.GetCache<ITimeCache>(type, evictionPolicy, staleWhileRevalidate);
        await sut.SetAsync("Key", value, TimeSpan.FromSeconds(1));
        sut.DataSetEvent += (_, _) => { _dataSetEventCount++; };
        sut.DataGetEvent += (_, _) => { _dataGetEventCount++; };
        sut.DataDropEvent += (_, _) => { _dataDropEventCount++; };

        //Act
        var item = await sut.PeekAsync<string>("Key");

        //Assert
        _dataSetEventCount.Should().Be(0);
        _dataGetEventCount.Should().Be(1);
        _dataDropEventCount.Should().Be(0);
        item.Should().Be(value);
    }

    [Theory]
    [ClassData(typeof(EndingCacheTypes))]
    public async Task SetValueAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var value = "value";
        var sut = CacheTypeLoader.GetCache<ITimeCache>(type, evictionPolicy, staleWhileRevalidate);
        await sut.SetAsync("Key", value, TimeSpan.FromSeconds(1));
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
    [ClassData(typeof(EndingCacheTypes))]
    public async Task DropValueAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var value = "value";
        var sut = CacheTypeLoader.GetCache<ITimeCache>(type, evictionPolicy, staleWhileRevalidate, TimeSpan.FromSeconds(1));
        await sut.SetAsync("Key", value, TimeSpan.FromSeconds(1));
        sut.DataSetEvent += (_, _) => { _dataSetEventCount++; };
        sut.DataGetEvent += (_, _) => { _dataGetEventCount++; };
        sut.DataDropEvent += (_, _) => { _dataDropEventCount++; };

        //Act
        var item = await sut.DropAsync<string>("Key");

        //Assert
        _dataSetEventCount.Should().Be(0);
        _dataGetEventCount.Should().Be(0);
        _dataDropEventCount.Should().Be(1);
        item.Should().Be(value);
    }
}