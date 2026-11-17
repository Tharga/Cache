using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Tharga.Cache.Core;
using Tharga.Cache.Tests.Helper;
using Xunit;

namespace Tharga.Cache.Tests;

public class WatchDogServiceTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-5000)]
    public async Task ZeroOrNegativeInterval_DoesNotRun(int milliseconds)
    {
        //Arrange
        var runs = 0;
        var monitor = new Mock<ICacheMonitor>();
        monitor.Setup(x => x.ClearStale()).Callback(() => Interlocked.Increment(ref runs));
        var logger = new RecordingLogger<WatchDogService>();
        var sut = new WatchDogService(monitor.Object, Options.Create(new CacheOptions { WatchDogInterval = TimeSpan.FromMilliseconds(milliseconds) }), logger);

        //Act
        await sut.StartAsync(CancellationToken.None);
        await Task.Delay(200);
        await sut.StopAsync(CancellationToken.None);

        //Assert
        runs.Should().Be(0);
        sut.ExecuteTask.IsCompletedSuccessfully.Should().BeTrue();
        logger.Entries.Count(x => x.Level == LogLevel.Warning).Should().Be(1);
    }

    [Fact]
    public async Task PositiveInterval_Runs()
    {
        //Arrange
        var runs = 0;
        var monitor = new Mock<ICacheMonitor>();
        monitor.Setup(x => x.ClearStale()).Callback(() => Interlocked.Increment(ref runs));
        var logger = new RecordingLogger<WatchDogService>();
        var sut = new WatchDogService(monitor.Object, Options.Create(new CacheOptions { WatchDogInterval = TimeSpan.FromMilliseconds(20) }), logger);

        //Act
        await sut.StartAsync(CancellationToken.None);
        await Task.Delay(300);
        await sut.StopAsync(CancellationToken.None);

        //Assert
        runs.Should().BeGreaterThan(1);
        logger.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task InfiniteInterval_RunsOnceAndWaits()
    {
        //Arrange
        var runs = 0;
        var monitor = new Mock<ICacheMonitor>();
        monitor.Setup(x => x.ClearStale()).Callback(() => Interlocked.Increment(ref runs));
        var logger = new RecordingLogger<WatchDogService>();
        var sut = new WatchDogService(monitor.Object, Options.Create(new CacheOptions { WatchDogInterval = Timeout.InfiniteTimeSpan }), logger);

        //Act
        await sut.StartAsync(CancellationToken.None);
        await Task.Delay(200);
        var completedWhileRunning = sut.ExecuteTask.IsCompleted;
        await sut.StopAsync(CancellationToken.None);

        //Assert
        runs.Should().Be(1);
        completedWhileRunning.Should().BeFalse();
        logger.Entries.Should().BeEmpty();
    }
}
