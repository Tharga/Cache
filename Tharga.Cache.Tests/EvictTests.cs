using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Tharga.Cache.Core;
using Tharga.Cache.Persist;
using Xunit;

namespace Tharga.Cache.Tests;

public class EvictTests
{
    public interface IFirstPersist : IPersist;
    public interface ISecondPersist : IPersist;

    [Fact]
    public void ClearStale_keeps_running_when_a_drop_fails()
    {
        //Arrange
        var first = new Mock<IPersist>();
        first.Setup(x => x.DropAsync<string>(It.IsAny<Key>())).ReturnsAsync(true);
        first.Setup(x => x.DropAsync<string>(It.Is<Key>(k => k.Value == "b"))).ThrowsAsync(new IOException("The file is locked."));
        var logger = new Mock<ILogger>();
        var sut = BuildMonitor(first.Object, Mock.Of<IPersist>(MockBehavior.Strict), logger.Object);
        SetItem(sut, "a", "x", stale: true);
        SetItem(sut, "b", "x", stale: true);
        SetItem(sut, "c", "x", stale: true);
        var unobserved = TrackUnobservedExceptions();

        //Act
        var act = () => sut.ClearStale();

        //Assert
        act.Should().NotThrow();
        sut.GetByType<string>().Keys.Should().BeEquivalentTo("b");
        first.Verify(x => x.DropAsync<string>(It.Is<Key>(k => k.Value == "a")), Times.Once);
        first.Verify(x => x.DropAsync<string>(It.Is<Key>(k => k.Value == "c")), Times.Once);
        logger.Verify(x => x.Log(LogLevel.Warning, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<IOException>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        unobserved().Should().BeEmpty();
    }

    [Fact]
    public void ClearAll_keeps_running_when_a_drop_throws_directly()
    {
        //Arrange
        var first = new Mock<IPersist>();
        first.Setup(x => x.DropAsync<string>(It.IsAny<Key>())).ReturnsAsync(true);
        first.Setup(x => x.DropAsync<string>(It.Is<Key>(k => k.Value == "a"))).Throws(new TimeoutException("No server available."));
        var sut = BuildMonitor(first.Object, Mock.Of<IPersist>(MockBehavior.Strict));
        SetItem(sut, "a", "x");
        SetItem(sut, "b", "x");
        var unobserved = TrackUnobservedExceptions();

        //Act
        var act = () => sut.ClearAll();

        //Assert
        act.Should().NotThrow();
        sut.GetByType<string>().Keys.Should().BeEquivalentTo("a");
        unobserved().Should().BeEmpty();
    }

    [Fact]
    public async Task ClearStaleAsync_completes_when_a_drop_fails_after_an_await()
    {
        //Arrange
        var first = new Mock<IPersist>();
        first.Setup(x => x.DropAsync<string>(It.IsAny<Key>())).ReturnsAsync(true);
        first.Setup(x => x.DropAsync<string>(It.Is<Key>(k => k.Value == "a"))).Returns(async () =>
        {
            await Task.Yield();
            throw new IOException("The file is locked.");
        });
        var sut = BuildMonitor(first.Object, Mock.Of<IPersist>(MockBehavior.Strict));
        SetItem(sut, "a", "x", stale: true);
        SetItem(sut, "b", "x", stale: true);

        //Act
        var act = () => sut.ClearStaleAsync();

        //Assert
        await act.Should().NotThrowAsync();
        sut.GetByType<string>().Keys.Should().BeEquivalentTo("a");
    }

    [Fact]
    public void ClearAll_drops_each_item_only_from_the_persist_of_its_type()
    {
        //Arrange
        var first = new Mock<IPersist>();
        first.Setup(x => x.DropAsync<string>(It.IsAny<Key>())).ReturnsAsync(true);
        var second = new Mock<IPersist>();
        second.Setup(x => x.DropAsync<int>(It.IsAny<Key>())).ReturnsAsync(true);
        var sut = BuildMonitor(first.Object, second.Object);
        SetItem(sut, "s", "x");
        SetItem(sut, "i", 1);

        //Act
        sut.ClearAll();

        //Assert
        first.Verify(x => x.DropAsync<string>(It.Is<Key>(k => k.Value == "s")), Times.Once);
        first.Verify(x => x.DropAsync<int>(It.IsAny<Key>()), Times.Never);
        second.Verify(x => x.DropAsync<int>(It.Is<Key>(k => k.Value == "i")), Times.Once);
        second.Verify(x => x.DropAsync<string>(It.IsAny<Key>()), Times.Never);
        sut.GetInfos().Should().BeEmpty();
    }

    private static CacheMonitor BuildMonitor(IPersist first, IPersist second, ILogger logger = null)
    {
        var options = new CacheOptions();
        options.RegisterType<string, IFirstPersist>();
        options.RegisterType<int, ISecondPersist>();

        var persistLoader = new Mock<IPersistLoader>(MockBehavior.Strict);
        persistLoader.Setup(x => x.GetPersist(typeof(IFirstPersist))).Returns(first);
        persistLoader.Setup(x => x.GetPersist(typeof(ISecondPersist))).Returns(second);

        return new CacheMonitor(persistLoader.Object, options, logger);
    }

    private static void SetItem<T>(CacheMonitor monitor, string key, T data, bool stale = false)
    {
        var item = stale
            ? CacheItemBuilder.BuildCacheItem([], data, TimeSpan.FromMilliseconds(1), createTime: DateTime.UtcNow.AddMinutes(-1))
            : CacheItemBuilder.BuildCacheItem([], data, null);
        monitor.Set(typeof(T), key, item, false, false);
    }

    private static Func<List<Exception>> TrackUnobservedExceptions()
    {
        var exceptions = new List<Exception>();
        TaskScheduler.UnobservedTaskException += (_, e) => exceptions.Add(e.Exception);
        return () =>
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            return exceptions;
        };
    }
}
