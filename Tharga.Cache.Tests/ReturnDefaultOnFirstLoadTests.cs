using FluentAssertions;
using Tharga.Cache.Core;
using Tharga.Cache.Persist;
using Tharga.Cache.Tests.Helper;
using Xunit;

namespace Tharga.Cache.Tests;

public class ReturnDefaultOnFirstLoadTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FirstCall_with_ReturnDefaultOnFirstLoad_returns_default(bool staleWhileRevalidate)
    {
        //Arrange
        var result = CacheTypeLoader.GetCache<ITimeCache, IMemory>(
            typeof(TimeToLiveCache), null, staleWhileRevalidate,
            returnDefaultOnFirstLoad: true);
        var sut = result.Cache;
        var fetchStarted = new TaskCompletionSource();
        var fetchGate = new TaskCompletionSource();

        //Act
        var item = await sut.GetAsync("Key", async () =>
        {
            fetchStarted.TrySetResult();
            await fetchGate.Task;
            return "fetched";
        });

        //Assert
        item.Should().BeNull("first call should return default(T) without blocking");
        await fetchStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        fetchGate.TrySetResult();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FirstCall_without_ReturnDefaultOnFirstLoad_blocks(bool staleWhileRevalidate)
    {
        //Arrange
        var result = CacheTypeLoader.GetCache<ITimeCache, IMemory>(
            typeof(TimeToLiveCache), null, staleWhileRevalidate,
            returnDefaultOnFirstLoad: false);
        var sut = result.Cache;

        //Act
        var item = await sut.GetAsync("Key", async () =>
        {
            await Task.Delay(50);
            return "fetched";
        });

        //Assert
        item.Should().Be("fetched", "first call should block until factory completes");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SecondCall_returns_fetched_data_after_background_load(bool staleWhileRevalidate)
    {
        //Arrange
        var result = CacheTypeLoader.GetCache<ITimeCache, IMemory>(
            typeof(TimeToLiveCache), null, staleWhileRevalidate,
            returnDefaultOnFirstLoad: true);
        var sut = result.Cache;

        //Act - first call returns default, factory runs in background
        var first = await sut.GetAsync("Key", () => Task.FromResult("fetched"));
        await Task.Delay(200);

        //Assert - second call should return the now-cached data
        var second = await sut.GetAsync("Key", () => Task.FromResult("should-not-be-called"));
        second.Should().Be("fetched");
    }

    [Fact]
    public async Task ReturnDefaultOnFirstLoad_with_existing_fresh_data_returns_cached()
    {
        //Arrange
        var result = CacheTypeLoader.GetCache<ITimeCache, IMemory>(
            typeof(TimeToLiveCache), null, staleWhileRevalidate: false,
            returnDefaultOnFirstLoad: true);
        var sut = result.Cache;
        await sut.SetAsync("Key", "cached");

        //Act
        var item = await sut.GetAsync("Key", () => Task.FromResult("should-not-be-called"));

        //Assert
        item.Should().Be("cached", "fresh cached data should be returned regardless of ReturnDefaultOnFirstLoad");
    }
}
