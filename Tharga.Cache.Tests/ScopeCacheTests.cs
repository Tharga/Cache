using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tharga.Cache.Tests;

public class ScopeCacheTests
{
    [Fact]
    public async Task Each_scope_keeps_its_own_value_for_the_same_key()
    {
        //Arrange
        using var provider = BuildProvider();
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();
        var first = firstScope.ServiceProvider.GetRequiredService<IScopeCache>();
        var second = secondScope.ServiceProvider.GetRequiredService<IScopeCache>();

        //Act
        var firstValue = await first.GetAsync("current-context", () => Task.FromResult("first"));
        var secondValue = await second.GetAsync("current-context", () => Task.FromResult("second"));

        //Assert
        firstValue.Should().Be("first");
        secondValue.Should().Be("second");
        (await first.PeekAsync<string>("current-context")).Should().Be("first");
        (await second.PeekAsync<string>("current-context")).Should().Be("second");
    }

    [Fact]
    public async Task Concurrent_loads_in_two_scopes_each_use_their_own_loader()
    {
        //Arrange
        using var provider = BuildProvider();
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();
        var first = firstScope.ServiceProvider.GetRequiredService<IScopeCache>();
        var second = secondScope.ServiceProvider.GetRequiredService<IScopeCache>();
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        //Act
        var firstTask = first.GetAsync("current-context", async () => { await release.Task; return "first"; });
        var secondTask = second.GetAsync("current-context", async () => { await release.Task; return "second"; });
        release.SetResult();

        //Assert
        (await firstTask).Should().Be("first");
        (await secondTask).Should().Be("second");
    }

    [Fact]
    public async Task Values_are_gone_after_the_scope_is_disposed()
    {
        //Arrange
        using var provider = BuildProvider();
        var scope = provider.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<IScopeCache>();
        await sut.SetAsync("current-context", "first");

        //Act
        scope.Dispose();

        //Assert
        (await sut.PeekAsync<string>("current-context")).Should().BeNull();
        using var nextScope = provider.CreateScope();
        var next = nextScope.ServiceProvider.GetRequiredService<IScopeCache>();
        (await next.PeekAsync<string>("current-context")).Should().BeNull();
    }

    [Fact]
    public async Task Scope_values_do_not_reach_the_eternal_cache()
    {
        //Arrange
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<IScopeCache>();
        var eternal = provider.GetRequiredService<IEternalCache>();

        //Act
        await sut.SetAsync("current-context", "scoped");

        //Assert
        (await eternal.PeekAsync<string>("current-context")).Should().BeNull();
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCache();
        return services.BuildServiceProvider();
    }
}
