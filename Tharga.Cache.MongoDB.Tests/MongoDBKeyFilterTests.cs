using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Moq;
using Tharga.Cache.Core;
using Tharga.MongoDB;
using Xunit;
using Sut = Tharga.Cache.MongoDB.MongoDB;

namespace Tharga.Cache.MongoDB.Tests;

/// <summary>
/// The cache key has to reach Mongo as an id, never as a filter document.
/// </summary>
/// <remarks>
/// DeleteOneAsync and UpdateOneAsync have no TKey overload, so a bare string is picked up by the
/// FilterDefinition overload through the driver's implicit string -> JsonFilterDefinition conversion.
/// That compiles cleanly and then fails at runtime, once per drop, with
/// "JSON reader was expecting a value but found '&lt;TypeName&gt;'" - because CacheBase prefixes every
/// key with the type name. Rendering the filter here is what tells the two apart.
/// </remarks>
public class MongoDBKeyFilterTests
{
    private const string CacheKey = "FeedStatusDto.my-feed-key";

    private static BsonDocument Render(FilterDefinition<CacheEntity> filter)
    {
        return filter.Render(new RenderArgs<CacheEntity>(BsonSerializer.SerializerRegistry.GetSerializer<CacheEntity>(), BsonSerializer.SerializerRegistry));
    }

    private static (Sut Sut, Mock<ICacheRepositoryCollection> Collection) BuildSut()
    {
        var collection = new Mock<ICacheRepositoryCollection>();

        var collectionProvider = new Mock<ICollectionProvider>();
        collectionProvider
            .Setup(x => x.GetCollection<ICacheRepositoryCollection, CacheEntity, string>(It.IsAny<DatabaseContext>()))
            .Returns(collection.Object);

        var sut = new Sut(collectionProvider.Object, Mock.Of<IManagedCacheMonitor>(), Options.Create(new MongoDBCacheOptions()), Mock.Of<ILogger<Sut>>());
        return (sut, collection);
    }

    [Fact]
    public async Task DropAsync_FiltersOnTheIdRatherThanParsingTheKeyAsJson()
    {
        var (sut, collection) = BuildSut();
        FilterDefinition<CacheEntity> captured = null;
        collection
            .Setup(x => x.DeleteOneAsync(It.IsAny<FilterDefinition<CacheEntity>>(), It.IsAny<OneOption<CacheEntity>>(), It.IsAny<IClientSessionHandle>()))
            .Callback<FilterDefinition<CacheEntity>, OneOption<CacheEntity>, IClientSessionHandle>((f, _, _) => captured = f)
            .ReturnsAsync((CacheEntity)null);

        await sut.DropAsync<object>(CacheKey);

        captured.Should().NotBeNull();
        var rendered = Render(captured);
        rendered.ElementCount.Should().Be(1);
        rendered.GetElement(0).Name.Should().Be("_id");
        rendered.GetElement(0).Value.AsString.Should().Be(CacheKey);
    }

}


