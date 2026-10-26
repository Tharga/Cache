using FluentAssertions;
using Moq;
using Tharga.Cache.Core;
using Tharga.Cache.Persist;
using Tharga.Cache.Tests.Helper;
using Xunit;

namespace Tharga.Cache.Tests;

public class TimeToIdleCacheTests
{
    private readonly ManualTimeProvider _clock = new();
    private readonly Mock<IPersistLoader> _persistLoader = new(MockBehavior.Strict);
    private readonly CacheOptions _options;
    private readonly CacheMonitor _cacheMonitor;
    private readonly IFetchQueue _fetchQueue;

    public TimeToIdleCacheTests()
    {
        _options = new CacheOptions { TimeProvider = _clock };
        _cacheMonitor = new CacheMonitor(_persistLoader.Object, _options);
        _persistLoader.Setup(x => x.GetPersist(It.IsAny<Type>())).Returns(new Memory(_cacheMonitor, _clock));
        _fetchQueue = new FetchQueue(_cacheMonitor, _options, null);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task KeepIfUsed(bool keep)
    {
        //Arrange
        var dataSetEventCount = 0;
        var dataGetEventCount = 0;
        var monitorSetEventCount = 0;
        var monitorGetEventCount = 0;
        var monitorDropEventCount = 0;
        var sut = new TimeToIdleCache(_cacheMonitor, _persistLoader.Object, _fetchQueue, _options);
        sut.DataSetEvent += (_, _) => Interlocked.Increment(ref dataSetEventCount);
        sut.DataGetEvent += (_, _) => Interlocked.Increment(ref dataGetEventCount);
        _cacheMonitor.DataSetEvent += (_, _) => Interlocked.Increment(ref monitorSetEventCount);
        _cacheMonitor.DataGetEvent += (_, _) => Interlocked.Increment(ref monitorGetEventCount);
        _cacheMonitor.DataDropEvent += (_, _) => Interlocked.Increment(ref monitorDropEventCount);

        //Act
        // The clock only moves when advanced, so the boundaries are exact. TTI=400ms; each read at t moves expiry to t+400.
        _ = await sut.GetAsync("a", () => Task.FromResult("a1"), TimeSpan.FromMilliseconds(400));
        _clock.Advance(TimeSpan.FromMilliseconds(250));
        _ = await sut.GetAsync("a", () => Task.FromResult("a2"), TimeSpan.FromMilliseconds(400)); // t=250: fresh, expiry moves to 650.
        _clock.Advance(TimeSpan.FromMilliseconds(250));
        if (keep) _ = await sut.GetAsync("a", () => Task.FromResult("a3"), TimeSpan.FromMilliseconds(400)); // t=500: fresh, expiry moves to 900.
        _clock.Advance(TimeSpan.FromMilliseconds(300));
        var result = await sut.GetAsync("a", () => Task.FromResult("a4"), TimeSpan.FromMilliseconds(400)); // t=800: kept alive returns a1, otherwise expired at 650.

        //Assert
        result.Should().Be(keep ? "a1" : "a4");
        dataSetEventCount.Should().Be(keep ? 1 : 2);
        dataGetEventCount.Should().Be(keep ? 4 : 3);
        monitorSetEventCount.Should().Be(keep ? 1 : 2);
        monitorGetEventCount.Should().Be(keep ? 4 : 3);
        monitorDropEventCount.Should().Be(0);
    }
}
