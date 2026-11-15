using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Tharga.Cache.Core;
using Xunit;

namespace Tharga.Cache.Redis.Tests;

public class FindNotSupportedTests
{
    [Fact]
    public void Redis_FindAsync_NamesTheBackendInsteadOfThrowingNotImplemented()
    {
        var sut = new Redis(Mock.Of<IServiceProvider>(), Mock.Of<IHostEnvironment>(), Mock.Of<IManagedCacheMonitor>(), Options.Create(new RedisCacheOptions()), Mock.Of<ILogger<Redis>>());

        var act = () => sut.FindAsync<string>(KeyBuilder.Set("A", "a1"));

        act.Should().Throw<NotSupportedException>().WithMessage("*Redis*key parts*");
    }

#pragma warning disable CS0618
    [Fact]
    public void MemoryWithRedis_FindAsync_NamesTheBackendInsteadOfThrowingNotImplemented()
    {
        var sut = new MemoryWithRedis(Mock.Of<Tharga.Cache.Persist.IMemory>(), Mock.Of<IRedis>(), Mock.Of<IManagedCacheMonitor>());

        var act = () => sut.FindAsync<string>(KeyBuilder.Set("A", "a1"));

        act.Should().Throw<NotSupportedException>().WithMessage("*MemoryWithRedis*key parts*");
    }
#pragma warning restore CS0618
}
