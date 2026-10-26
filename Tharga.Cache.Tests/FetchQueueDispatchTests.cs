using FluentAssertions;
using Moq;
using Tharga.Cache.Core;
using Xunit;

namespace Tharga.Cache.Tests;

public class FetchQueueDispatchTests
{
    /// <summary>
    /// A fetch queued while the dispatcher is stopping must still run.
    /// </summary>
    /// <remarks>
    /// The dispatcher used to stop after finding the queue empty without looking again under its lock,
    /// so a fetch pushed in that window was never run and its caller waited forever. The window is narrow,
    /// so the test repeats the interleaving: before the fix, 5000 rounds left 2 to 6 fetches stranded.
    /// It cannot fail on a correct queue; the timeout only bounds how long a stranded fetch is waited for.
    /// </remarks>
    [Fact]
    public async Task FetchQueuedWhileTheDispatcherStopsStillRuns()
    {
        var stranded = 0;
        for (var i = 0; i < 5000; i++)
        {
            var sut = new FetchQueue(Mock.Of<IManagedCacheMonitor>(), new CacheOptions(), null);

            var first = sut.LoadData<int>("first", () => Task.FromResult(1), null, (_, _, _) => Task.CompletedTask);
            Thread.SpinWait(i % 200);
            var second = sut.LoadData<int>("second", () => Task.FromResult(2), null, (_, _, _) => Task.CompletedTask);

            try
            {
                await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch (TimeoutException)
            {
                stranded++;
                break;
            }
        }

        stranded.Should().Be(0, "every queued fetch must eventually run");
    }
}
