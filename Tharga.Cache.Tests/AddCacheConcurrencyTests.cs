using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tharga.Cache.Persist;
using Xunit;

namespace Tharga.Cache.Tests;

public class AddCacheConcurrencyTests
{
    private const int HostCount = 64;

    private sealed record Marker<T>;

    // Nesting a generic in itself yields as many distinct cache types as needed without
    // declaring one class per host.
    private static Type MarkerType(int depth)
    {
        var type = typeof(object);
        for (var i = 0; i < depth; i++)
        {
            type = typeof(Marker<>).MakeGenericType(type);
        }

        return type;
    }

    private static void RegisterMarker(CacheOptions options, int depth)
    {
        typeof(CacheOptions)
            .GetMethod(nameof(CacheOptions.RegisterType))!
            .MakeGenericMethod(MarkerType(depth), typeof(IMemory))
            .Invoke(options, [null]);
    }

    private static IServiceCollection BuildHost(int depth)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCache(o => RegisterMarker(o, depth));
        return services;
    }

    private static IReadOnlyDictionary<Type, CacheTypeOptions> RegisteredTypes(IServiceCollection services)
    {
        var options = services
            .Last(x => x.ServiceType == typeof(IOptions<CacheOptions>))
            .ImplementationInstance as IOptions<CacheOptions>;

        return options!.Value.GetRegistered();
    }

    [Fact]
    public void AddCache_CalledConcurrentlyOnIndependentCollections_DoesNotThrow()
    {
        //Arrange
        var depths = Enumerable.Range(1, HostCount).ToArray();

        //Act
        var act = () => Parallel.ForEach(depths, depth => BuildHost(depth));

        //Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AddCache_OnIndependentCollections_DoesNotShareRegistrations()
    {
        //Arrange
        var first = BuildHost(1);

        //Act
        var second = BuildHost(2);

        //Assert
        RegisteredTypes(first).Keys.Should().BeEquivalentTo([MarkerType(1)]);
        RegisteredTypes(second).Keys.Should().BeEquivalentTo([MarkerType(2)]);
    }

    [Fact]
    public void AddCache_CalledConcurrently_EachCollectionKeepsOnlyItsOwnType()
    {
        //Arrange
        var depths = Enumerable.Range(1, HostCount).ToArray();
        var hosts = new IServiceCollection[HostCount];

        //Act
        Parallel.ForEach(depths, depth => hosts[depth - 1] = BuildHost(depth));

        //Assert
        foreach (var depth in depths)
        {
            RegisteredTypes(hosts[depth - 1]).Keys.Should().BeEquivalentTo([MarkerType(depth)]);
        }
    }
}
