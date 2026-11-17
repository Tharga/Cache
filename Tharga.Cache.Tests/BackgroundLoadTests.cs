using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Tharga.Cache.Core;
using Tharga.Cache.Persist;
using Tharga.Cache.Tests.Helper;
using Xunit;

namespace Tharga.Cache.Tests;

public class BackgroundLoadTests
{
    [Fact]
    public async Task Callback_that_throws_is_logged_and_not_left_unobserved()
    {
        //Arrange
        var logger = new RecordingLogger<BackgroundLoadTests>();
        var sut = BuildCache(logger);
        var unobserved = TrackUnobservedExceptions();

        //Act
        var result = await sut.GetWithCallbackAsync("Key", () => Task.FromResult("fetched"), ThrowingCallback, null);

        //Assert
        result.Data.Should().BeNull("the first call returns default and loads in the background");
        (await logger.WaitForAsync(x => x.Exception?.Message == "callback-boom", TimeSpan.FromSeconds(5))).Should().BeTrue();
        logger.Entries.Single(x => x.Exception?.Message == "callback-boom").Level.Should().BeOneOf(LogLevel.Warning, LogLevel.Error);
        unobserved().Where(x => x.ToString().Contains("callback-boom")).Should().BeEmpty();
    }

    [Fact]
    public async Task Fetch_that_fails_is_logged_and_not_left_unobserved()
    {
        //Arrange
        var logger = new RecordingLogger<BackgroundLoadTests>();
        var sut = BuildCache(logger);
        var callbackCalled = false;
        var unobserved = TrackUnobservedExceptions();

        //Act
        await sut.GetWithCallbackAsync<string>("Key", () => throw new InvalidOperationException("fetch-boom"), _ =>
        {
            callbackCalled = true;
            return Task.CompletedTask;
        }, null);

        //Assert
        (await logger.WaitForAsync(x => x.Exception?.Message == "fetch-boom", TimeSpan.FromSeconds(5))).Should().BeTrue();
        callbackCalled.Should().BeFalse();
        unobserved().Where(x => x.ToString().Contains("fetch-boom")).Should().BeEmpty();
    }

    [Fact]
    public async Task Callback_that_succeeds_receives_the_data_and_nothing_is_logged()
    {
        //Arrange
        var logger = new RecordingLogger<BackgroundLoadTests>();
        var sut = BuildCache(logger);
        var received = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

        //Act
        await sut.GetWithCallbackAsync("Key", () => Task.FromResult("fetched"), x =>
        {
            received.TrySetResult(x);
            return Task.CompletedTask;
        }, null);

        //Assert
        (await received.Task.WaitAsync(TimeSpan.FromSeconds(5))).Should().Be("fetched");
        logger.Entries.Should().BeEmpty();
    }

    private static async Task ThrowingCallback(string data)
    {
        await Task.Yield();
        throw new InvalidOperationException("callback-boom");
    }

    private static TimeToLiveCache BuildCache(ILogger logger)
    {
        var options = new CacheOptions();
        options.RegisterType<string, IMemory>(s =>
        {
            s.ReturnDefaultOnFirstLoad = true;
            s.DefaultFreshSpan = TimeSpan.FromSeconds(10);
        });

        var persistLoader = new Mock<IPersistLoader>(MockBehavior.Strict);
        var cacheMonitor = new CacheMonitor(persistLoader.Object, options);
        persistLoader.Setup(x => x.GetPersist(It.IsAny<Type>())).Returns(new Memory(cacheMonitor));
        var fetchQueue = new FetchQueue(cacheMonitor, options, null);
        return new TimeToLiveCache(cacheMonitor, persistLoader.Object, fetchQueue, options, logger);
    }

    private static Func<List<Exception>> TrackUnobservedExceptions()
    {
        var exceptions = new List<Exception>();
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            lock (exceptions) exceptions.Add(e.Exception);
        };
        return () =>
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            lock (exceptions) return exceptions.ToList();
        };
    }
}
