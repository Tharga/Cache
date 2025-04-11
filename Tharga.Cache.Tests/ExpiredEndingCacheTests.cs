using FluentAssertions;
using Tharga.Cache.Tests.Helper;
using Xunit;

namespace Tharga.Cache.Tests;

public class ExpiredEndingCacheTests
{
    private int _dataSetEventCount;
    private int _dataGetEventCount;
    private int _dataDropEventCount;
    private int _monitorSetEventCount;

    [Theory]
    [ClassData(typeof(EndingCacheTypes))]
    [Trait("Category", "TimeCritical")]
    public async Task GetValueAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var value = "value";
        var result = CacheTypeLoader.GetCache<ITimeCache>(type, evictionPolicy, staleWhileRevalidate);
        var sut = result.Cache;
        await sut.SetAsync("Key", value, TimeSpan.FromMilliseconds(100));
        sut.DataSetEvent += (_, _) => { _dataSetEventCount++; };
        sut.DataGetEvent += (_, _) => { _dataGetEventCount++; };
        sut.DataDropEvent += (_, _) => { _dataDropEventCount++; };
        result.Monitor.DataSetEvent += (_, _) => { _monitorSetEventCount++; };
        await Task.Delay(500);
        result.Monitor.CleanSale();

        //Act
        var item = await sut.GetAsync("Key", async () =>
        {
            await Task.Delay(100);
            return "updated";
        });

        //Assert
        if (staleWhileRevalidate)
        {
            _dataSetEventCount.Should().Be(0);
            _dataGetEventCount.Should().Be(1);
            _dataDropEventCount.Should().Be(0);
            _monitorSetEventCount.Should().Be(0);
            item.Should().Be(value);
        }
        else
        {
            _dataSetEventCount.Should().Be(1);
            _dataGetEventCount.Should().Be(1);
            _dataDropEventCount.Should().Be(1);
            _monitorSetEventCount.Should().Be(1);
            item.Should().Be("updated");
        }
        result.Monitor.GetInfos().SelectMany(x => x.Items).Sum(x => x.Value.Size).Should().BeGreaterThan(0);
    }

    [Theory]
    [ClassData(typeof(EndingCacheTypes))]
    public async Task PeekValueAsync(Type type, EvictionPolicy? evictionPolicy, bool staleWhileRevalidate)
    {
        //Arrange
        var value = "value";
        var result = CacheTypeLoader.GetCache<ITimeCache>(type, evictionPolicy, staleWhileRevalidate);
        var sut = result.Cache;
        await sut.SetAsync("Key", value, TimeSpan.FromMilliseconds(100));
        sut.DataSetEvent += (_, _) => { _dataSetEventCount++; };
        sut.DataGetEvent += (_, _) => { _dataGetEventCount++; };
        sut.DataDropEvent += (_, _) => { _dataDropEventCount++; };
        result.Monitor.DataSetEvent += (_, _) => { _monitorSetEventCount++; };
        await Task.Delay(500);

        //Act
        var item = await sut.PeekAsync<string>("Key");

        //Assert
        _dataSetEventCount.Should().Be(0);
        _monitorSetEventCount.Should().Be(0);
        if (staleWhileRevalidate)
        {
            _dataDropEventCount.Should().Be(0);
            _dataGetEventCount.Should().Be(1);
            item.Should().Be(value);
            result.Monitor.GetInfos().SelectMany(x => x.Items).Sum(x => x.Value.Size).Should().BeGreaterThan(0);
        }
        else
        {
            _dataDropEventCount.Should().Be(1);
            _dataGetEventCount.Should().Be(0);
            item.Should().Be(default);
            result.Monitor.GetInfos().SelectMany(x => x.Items).Sum(x => x.Value.Size).Should().Be(0);
        }
    }
}