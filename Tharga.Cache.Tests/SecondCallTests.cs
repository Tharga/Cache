using System.Diagnostics;
using FluentAssertions;
using Tharga.Cache.Core;
using Tharga.Cache.Tests.Helper;
using Xunit;

namespace Tharga.Cache.Tests;

public class UnloadDataTests
{
    [Fact]
    public async Task UnloadOnCount()
    {
        //Arrange
        var dataDropEventCount = 0;
        var options = new Options();
        options.RegisterType<string>(s => s.MaxCount = 3);
        var cacheMonitor = new CacheMonitor();
        var sut = new EternalCache(cacheMonitor, new Memory(), options);
        sut.DataDropEvent += (s, e) =>
        {
            dataDropEventCount++;
        };
        await sut.SetAsync("a", "aa");
        await sut.SetAsync("b", "bb");
        await sut.SetAsync("c", "cc");

        //Act
        await sut.SetAsync("d", "dd");

        //Assert
        dataDropEventCount.Should().Be(1);
        //(await sut.GetAsync<string>().toas).
        cacheMonitor.GetInfos().Single().Items.Count.Should().Be(3);
        cacheMonitor.GetInfos().SelectMany(x => x.Items).Sum(x => x.Value.Size).Should().BeGreaterThan(0);
    }
}

public class SecondCallTests
{
    private int _dataSetEventCount;
    private int _dataGetEventCount;
    private int _dataDropEventCount;
    private int _monitorSetEventCount;

    [Theory]
    [ClassData(typeof(AllTypes))]
    public async Task GetValueAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var value = "value";
        var result = CacheTypeLoader.GetCache(type, evictionPolicy, staleWhileRevalidate);
        var sut = result.Cache;
        await sut.SetAsync("Key", value);
        sut.DataSetEvent += (_, _) => { _dataSetEventCount++; };
        sut.DataGetEvent += (_, _) => { _dataGetEventCount++; };
        sut.DataDropEvent += (_, _) => { _dataDropEventCount++; };
        result.Monitor.DataSetEvent += (_, _) => { _monitorSetEventCount++; };

        //Act
        var item = await sut.GetAsync("Key", () => Task.FromResult(value));

        //Assert
        _dataSetEventCount.Should().Be(0);
        _dataGetEventCount.Should().Be(1);
        _dataDropEventCount.Should().Be(0);
        _monitorSetEventCount.Should().Be(0);
        item.Should().Be(value);
        result.Monitor.GetInfos().SelectMany(x => x.Items).Sum(x => x.Value.Size).Should().BeGreaterThan(0);
    }

    [Theory]
    [ClassData(typeof(AllTypes))]
    public async Task PeekValueAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var value = "value";
        var result = CacheTypeLoader.GetCache(type, evictionPolicy, staleWhileRevalidate);
        var sut = result.Cache;
        await sut.SetAsync("Key", value);
        sut.DataSetEvent += (_, _) => { _dataSetEventCount++; };
        sut.DataGetEvent += (_, _) => { _dataGetEventCount++; };
        sut.DataDropEvent += (_, _) => { _dataDropEventCount++; };
        result.Monitor.DataSetEvent += (_, _) => { _monitorSetEventCount++; };

        //Act
        var item = await sut.PeekAsync<string>("Key");

        //Assert
        _dataSetEventCount.Should().Be(0);
        _dataGetEventCount.Should().Be(1);
        _dataDropEventCount.Should().Be(0);
        _monitorSetEventCount.Should().Be(0);
        item.Should().Be(value);
        result.Monitor.GetInfos().SelectMany(x => x.Items).Sum(x => x.Value.Size).Should().BeGreaterThan(0);
    }

    [Theory]
    [ClassData(typeof(AllTypes))]
    public async Task SetValueAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var value = "value";
        var result = CacheTypeLoader.GetCache(type, evictionPolicy, staleWhileRevalidate);
        var sut = result.Cache;
        await sut.SetAsync("Key", value);
        sut.DataSetEvent += (_, _) => { _dataSetEventCount++; };
        sut.DataGetEvent += (_, _) => { _dataGetEventCount++; };
        sut.DataDropEvent += (_, _) => { _dataDropEventCount++; };
        result.Monitor.DataSetEvent += (_, _) => { _monitorSetEventCount++; };

        //Act
        await sut.SetAsync("Key", value);

        //Assert
        _dataSetEventCount.Should().Be(1);
        _dataGetEventCount.Should().Be(0);
        _dataDropEventCount.Should().Be(0);
        _monitorSetEventCount.Should().Be(1);
        var item = await sut.GetAsync("Key", () => Task.FromResult("crap"));
        item.Should().Be(value);
        result.Monitor.GetInfos().SelectMany(x => x.Items).Sum(x => x.Value.Size).Should().BeGreaterThan(0);
    }

    [Theory]
    [ClassData(typeof(AllTypes))]
    public async Task DropValueAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var value = "value";
        var result = CacheTypeLoader.GetCache(type, evictionPolicy, staleWhileRevalidate);
        var sut = result.Cache;
        await sut.SetAsync("Key", value);
        sut.DataSetEvent += (_, _) => { _dataSetEventCount++; };
        sut.DataGetEvent += (_, _) => { _dataGetEventCount++; };
        sut.DataDropEvent += (_, _) => { _dataDropEventCount++; };
        result.Monitor.DataSetEvent += (_, _) => { _monitorSetEventCount++; };

        //Act
        var item = await sut.DropAsync<string>("Key");

        //Assert
        _dataSetEventCount.Should().Be(0);
        _dataGetEventCount.Should().Be(0);
        _dataDropEventCount.Should().Be(1);
        _monitorSetEventCount.Should().Be(0);
        item.Should().Be(value);
        result.Monitor.GetInfos().SelectMany(x => x.Items).Sum(x => x.Value.Size).Should().Be(0);
    }
}