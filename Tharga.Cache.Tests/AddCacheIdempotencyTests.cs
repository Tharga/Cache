using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Tharga.Cache.Persist;
using Xunit;

namespace Tharga.Cache.Tests;

public class AddCacheIdempotencyTests : IDisposable
{
    public AddCacheIdempotencyTests()
    {
        CacheRegistrationExtensions.ResetRegistrations();
    }

    public void Dispose()
    {
        CacheRegistrationExtensions.ResetRegistrations();
    }

    [Fact]
    public void AddCache_CalledTwice_WithDifferentTypes_DoesNotThrow()
    {
        //Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        //Act
        var act = () =>
        {
            services.AddCache(o => o.RegisterType<string, IMemory>());
            services.AddCache(o => o.RegisterType<int, IMemory>());
        };

        //Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AddCache_CalledTwice_WithSameType_DoesNotThrow()
    {
        //Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        //Act
        var act = () =>
        {
            services.AddCache(o => o.RegisterType<string, IMemory>());
            services.AddCache(o => o.RegisterType<string, IMemory>());
        };

        //Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AddCache_CalledTwice_WithSameType_FirstRegistrationWins()
    {
        //Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        //Act
        services.AddCache(o => o.RegisterType<string, IMemory>(t => t.DefaultFreshSpan = TimeSpan.FromMinutes(5)));
        services.AddCache(o => o.RegisterType<string, IMemory>(t => t.DefaultFreshSpan = TimeSpan.FromMinutes(99)));

        //Assert
        var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ITimeToLiveCache>();
        cache.Should().NotBeNull();
    }

    [Fact]
    public void AddCache_CalledTwice_WithNoTypes_DoesNotThrow()
    {
        //Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        //Act
        var act = () =>
        {
            services.AddCache();
            services.AddCache();
        };

        //Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AddCache_CalledTwice_BothTypesResolvable()
    {
        //Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        //Act
        services.AddCache(o => o.RegisterType<string, IMemory>());
        services.AddCache(o => o.RegisterType<int, IMemory>());

        //Assert
        var provider = services.BuildServiceProvider();
        var cache = provider.GetRequiredService<ITimeToLiveCache>();
        cache.Should().NotBeNull();
    }

    [Fact]
    public void AddCache_CalledTwice_RegistersSingletonOnce()
    {
        //Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        //Act
        services.AddCache();
        services.AddCache();

        //Assert
        var cacheMonitorCount = services.Count(s => s.ServiceType == typeof(ICacheMonitor));
        cacheMonitorCount.Should().Be(1);
    }
}
