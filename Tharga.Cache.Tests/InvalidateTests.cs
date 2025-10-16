using FluentAssertions;
using Tharga.Cache.Tests.Helper;
using Xunit;

namespace Tharga.Cache.Tests;

public class InvalidateTests
{
    private int _dataSetEventCount;
    private int _dataGetEventCount;
    private int _dataDropEventCount;
    private int _monitorSetEventCount;
    private int _monitorGetEventCount;
    private int _monitorDropEventCount;

    [Theory]
    [ClassData(typeof(AllTypes))]
    public async Task GetAfterInvalidate(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
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
        result.Monitor.DataGetEvent += (_, _) => { _monitorGetEventCount++; };
        result.Monitor.DataDropEvent += (_, _) => { _monitorDropEventCount++; };
        await sut.InvalidateAsync<string>("Key");

        //Act
        var item = await sut.GetAsync("Key", () => Task.FromResult(value));

        //Assert
        _dataSetEventCount.Should().Be(staleWhileRevalidate ? 0 : 1);
        _dataGetEventCount.Should().Be(1);
        _dataDropEventCount.Should().Be(staleWhileRevalidate ? 0 : 1);
        _monitorSetEventCount.Should().Be(staleWhileRevalidate ? 0 : 1);
        _monitorGetEventCount.Should().Be(1);
        _monitorDropEventCount.Should().Be(staleWhileRevalidate ? 0 : 1);
        item.Should().Be(value);
        result.Monitor.GetInfos().SelectMany(x => x.Items).Sum(x => x.Value.Size).Should().BeGreaterThan(0);
    }

    [Theory]
    [ClassData(typeof(AllTypes))]
    public async Task PeekAfterInvalidateAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
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
        result.Monitor.DataGetEvent += (_, _) => { _monitorGetEventCount++; };
        result.Monitor.DataDropEvent += (_, _) => { _monitorDropEventCount++; };
        await sut.InvalidateAsync<string>("Key");

        //Act
        var item = await sut.PeekAsync<string>("Key");

        //Assert
        _dataSetEventCount.Should().Be(0);
        _dataGetEventCount.Should().Be(staleWhileRevalidate ? 1 : 0);
        _dataDropEventCount.Should().Be(staleWhileRevalidate ? 0 : 1);
        _monitorSetEventCount.Should().Be(0);
        _monitorGetEventCount.Should().Be(staleWhileRevalidate ? 1 : 0);
        _monitorDropEventCount.Should().Be(staleWhileRevalidate ? 0 : 1);
        if (staleWhileRevalidate)
        {
            item.Should().Be(value);
            result.Monitor.GetInfos().SelectMany(x => x.Items).Sum(x => x.Value.Size).Should().BeGreaterThan(0);
        }
        else
        {
            item.Should().BeNull();
            result.Monitor.GetInfos().SelectMany(x => x.Items).Sum(x => x.Value.Size).Should().Be(0);
        }
    }

    [Theory]
    [ClassData(typeof(AllTypes))]
    public async Task InvalidateAllByTypeAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var result = CacheTypeLoader.GetCache(type, evictionPolicy, staleWhileRevalidate);
        var sut = result.Cache;
        await sut.SetAsync("Key1", "a");
        await sut.SetAsync("Key2", "b");
        await sut.SetAsync("Key3", 1);

        //Act
        var item = await sut.InvalidateAsync<string>(KeyBuilder.Empty);

        //Assert
        item.Should().Be(2);
        if (staleWhileRevalidate)
        {
            result.Monitor.GetInfos().SelectMany(x => x.Items).Count().Should().Be(3);
        }
        else
        {
            result.Monitor.GetInfos().SelectMany(x => x.Items).Count().Should().Be(1);
        }
        sut.PeekAsync<string>("Key1").Should().NotBeNull();
        sut.PeekAsync<string>("Key2").Should().NotBeNull();
        sut.PeekAsync<int>("Key3").Should().NotBeNull();
    }

    [Theory]
    [ClassData(typeof(AllTypes))]
    public async Task InvalidateByKeyPartTypeAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var result = CacheTypeLoader.GetCache(type, evictionPolicy, staleWhileRevalidate);
        var sut = result.Cache;
        await sut.SetAsync(KeyBuilder.Set("Ax", "a1").Set("Bx", "b1"), "a");
        await sut.SetAsync(KeyBuilder.Set("Ax", "a1").Set("Bx", "b2"), "b");
        await sut.SetAsync(KeyBuilder.Set("Ax", "a2").Set("Bx", "b3"), "c");

        //Act
        var item = await sut.InvalidateAsync<string>(KeyBuilder.Set("Ax", "a1"));

        //Assert
        item.Should().Be(2);
        if (staleWhileRevalidate)
        {
            result.Monitor.GetInfos().SelectMany(x => x.Items).Count().Should().Be(3);
        }
        else
        {
            result.Monitor.GetInfos().SelectMany(x => x.Items).Count().Should().Be(1);
        }
    }
}