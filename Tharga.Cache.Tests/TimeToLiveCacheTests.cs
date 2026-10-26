using FluentAssertions;
using Moq;
using Tharga.Cache.Core;
using Tharga.Cache.Persist;
using Tharga.Cache.Tests.Helper;
using Xunit;

namespace Tharga.Cache.Tests;

public class TimeToLiveCacheTests
{
    private static readonly TimeSpan FreshSpan = TimeSpan.FromMilliseconds(400);

    private readonly ManualTimeProvider _clock = new();
    private readonly Mock<IPersistLoader> _persistLoader = new(MockBehavior.Strict);
    private readonly CacheOptions _options;
    private readonly CacheMonitor _cacheMonitor;
    private readonly TimeToLiveCache _sut;

    public TimeToLiveCacheTests()
    {
        _options = new CacheOptions { TimeProvider = _clock };
        _cacheMonitor = new CacheMonitor(_persistLoader.Object, _options);
        _persistLoader.Setup(x => x.GetPersist(It.IsAny<Type>())).Returns(new Memory(_cacheMonitor, _clock));
        _sut = new TimeToLiveCache(_cacheMonitor, _persistLoader.Object, new FetchQueue(_cacheMonitor, _options, null), _options);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DropEvenIfUsed(bool keep)
    {
        //Arrange
        var dataSetEventCount = 0;
        var dataGetEventCount = 0;
        var monitorSetEventCount = 0;
        var monitorGetEventCount = 0;
        var monitorDropEventCount = 0;
        _sut.DataSetEvent += (_, _) => Interlocked.Increment(ref dataSetEventCount);
        _sut.DataGetEvent += (_, _) => Interlocked.Increment(ref dataGetEventCount);
        _cacheMonitor.DataSetEvent += (_, _) => Interlocked.Increment(ref monitorSetEventCount);
        _cacheMonitor.DataGetEvent += (_, _) => Interlocked.Increment(ref monitorGetEventCount);
        _cacheMonitor.DataDropEvent += (_, _) => Interlocked.Increment(ref monitorDropEventCount);

        //Act
        // The clock only moves when advanced, so the boundaries are exact. TTL=400ms, a1 is stored at t=0.
        _ = await _sut.GetAsync("a", () => Task.FromResult("a1"), FreshSpan);
        _clock.Advance(TimeSpan.FromMilliseconds(250));
        var whileFresh = await _sut.GetAsync("a", () => Task.FromResult("a2"), FreshSpan); // t=250: a1 is fresh, and reading it does not extend it.
        _clock.Advance(TimeSpan.FromMilliseconds(250));
        if (keep) _ = await _sut.GetAsync("a", () => Task.FromResult("a3"), FreshSpan); // t=500: a1 expired although it was read, so a3 is fetched.
        _clock.Advance(TimeSpan.FromMilliseconds(300));
        var result = await _sut.GetAsync("a", () => Task.FromResult("a4"), FreshSpan); // t=800: a3 (stored at t=500) is fresh, a1 is not.

        //Assert
        whileFresh.Should().Be("a1");
        result.Should().Be(keep ? "a3" : "a4");
        dataSetEventCount.Should().Be(2);
        dataGetEventCount.Should().Be(keep ? 4 : 3);
        monitorSetEventCount.Should().Be(2);
        monitorGetEventCount.Should().Be(keep ? 4 : 3);
        monitorDropEventCount.Should().Be(0);
    }

    [Fact]
    public async Task ExpiresAsSoonAsTheFreshSpanHasPassed()
    {
        //Act
        _ = await _sut.GetAsync("a", () => Task.FromResult("a1"), FreshSpan);
        _clock.Advance(FreshSpan);
        var atBoundary = await _sut.GetAsync("a", () => Task.FromResult("a2"), FreshSpan);
        _clock.Advance(TimeSpan.FromTicks(1));
        var pastBoundary = await _sut.GetAsync("a", () => Task.FromResult("a3"), FreshSpan);

        //Assert
        atBoundary.Should().Be("a1", "an item is fresh up to and including its fresh span");
        pastBoundary.Should().Be("a3", "an item is stale as soon as its fresh span has passed");
    }
}
