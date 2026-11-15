using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Tharga.Cache.File.Tests;

public sealed class FindTests : IDisposable
{
    private readonly string _appName = $"Tharga.Cache.FindTests.{Guid.NewGuid():N}";
    private readonly File _sut;

    public FindTests()
    {
        var options = Options.Create(new FileCacheOptions { CompanyName = "Tharga", AppName = _appName });
        var hostEnvironment = new Mock<IHostEnvironment>();
        hostEnvironment.Setup(x => x.EnvironmentName).Returns(Environments.Production);
        _sut = new File(null, new FileService(), new FileFormatService(options, NullLogger<FileFormatService>.Instance), hostEnvironment.Object, options);
    }

    [Fact]
    public async Task FindByKeyPartsReturnsOnlyMatchingItems()
    {
        await Set("Key1", "a", ("A", "a1"), ("B", "b1"));
        await Set("Key2", "b", ("A", "a1"), ("B", "b2"));
        await Set("Key3", "c", ("A", "a2"), ("B", "b3"));

        var found = await Find<string>(KeyBuilder.Set("A", "a1"));

        found.Select(x => x.Data).Should().BeEquivalentTo("a", "b");
        found.Select(x => x.Key.Value).Should().BeEquivalentTo("Key1", "Key2");
    }

    [Fact]
    public async Task FindWithoutKeyPartsReturnsEveryItemOfTheType()
    {
        await Set("Key1", "a");
        await Set("Key2", "b");
        await _sut.SetAsync("Key3", Item(1), false);

        var found = await Find<string>(KeyBuilder.Empty);

        found.Select(x => x.Data).Should().BeEquivalentTo("a", "b");
    }

    [Fact]
    public async Task FindOnTypeWithNoFilesReturnsNothing()
    {
        var found = await Find<string>(KeyBuilder.Set("A", "a1"));

        found.Should().BeEmpty();
    }

    [Fact]
    public async Task KeyReturnedByFindCanBeDropped()
    {
        await Set("Key.With.Dots", "a", ("A", "a1"));
        await Set("Key2", "b", ("A", "a2"));

        var found = await Find<string>(KeyBuilder.Set("A", "a1"));
        foreach (var item in found)
        {
            (await _sut.DropAsync<string>(item.Key)).Should().BeTrue();
        }

        (await _sut.GetAsync<string>("Key.With.Dots")).Should().BeNull();
        (await _sut.GetAsync<string>("Key2")).Should().NotBeNull();
    }

    [Fact]
    public async Task FindWithoutKeyReturnsEveryItemOfTheType()
    {
        await Set("Key1", "a", ("A", "a1"));

        var found = await Find<string>(null);

        found.Should().HaveCount(1);
    }

    private Task Set(string key, string data, params (string Name, string Value)[] parts)
    {
        return _sut.SetAsync(key, Item(data, parts), false);
    }

    private static CacheItem<T> Item<T>(T data, params (string Name, string Value)[] parts)
    {
        return new CacheItem<T>
        {
            KeyParts = parts.ToDictionary(x => x.Name, x => x.Value),
            CreateTime = DateTime.UtcNow,
            Data = data
        };
    }

    private async Task<List<(Key Key, T Data)>> Find<T>(Key key)
    {
        var result = new List<(Key, T)>();
        await foreach (var item in _sut.FindAsync<T>(key))
        {
            result.Add((item.Key, item.CacheItem.Data));
        }

        return result;
    }

    public void Dispose()
    {
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Tharga", _appName);
        if (Directory.Exists(directory)) Directory.Delete(directory, true);
    }
}
