using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Attributes;
using Tharga.MongoDB;
using Tharga.MongoDB.Configuration;
using Tharga.MongoDB.Disk;

namespace Tharga.Cache.MongoDB;

public interface ICacheRepository : IRepository
{
}

internal class CacheRepository : ICacheRepository
{
}

internal interface ICacheRepositoryCollection : IDiskRepositoryCollection<CacheEntity, string>
{
}

internal class CacheRepositoryCollection : DiskRepositoryCollectionBase<CacheEntity, string>, ICacheRepositoryCollection
{
    public CacheRepositoryCollection(IMongoDbServiceFactory mongoDbServiceFactory, ILogger<CacheRepositoryCollection> logger, DatabaseContext databaseContext)
        : base(mongoDbServiceFactory, logger, databaseContext)
    {
    }
}

public record CacheEntity : EntityBase<string>
{
    public required string Type { get; init; }
    public required string Data { get; init; }
    public required DateTime CreateTime { get; init; }

    [BsonIgnoreIfDefault]
    public DateTime? UpdateTime { get; init; }

    [BsonIgnoreIfDefault]
    public TimeSpan? FreshSpan { get; init; }

    [BsonIgnoreIfDefault]
    public required bool StaleWhileRevalidate { get; init; }
}

public record MongoDBCacheOptions
{
    public string CollectionName { get; set; } = "Cache";
    public string ConfigurationName { get; set; }
}

internal class MongoDB : IMongoDB
{
    private readonly ICollectionProvider _collectionProvider;
    private readonly MongoDBCacheOptions _options;

    public MongoDB(ICollectionProvider collectionProvider, IOptions<MongoDBCacheOptions> options)
    {
        _collectionProvider = collectionProvider;
        _options = options.Value;
    }

    public Task<(bool Success, string Message)> CanConnectAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<CacheItem<T>> GetAsync<T>(Key key)
    {
        var collection = GetCollection();

        OneOption<CacheEntity> option = OneOption<CacheEntity>.SingleOrDefault;
        var item = await collection.GetOneAsync(x => x.Id == key.Value, option); //TODO: Should be possible to provide option with ID (not predicate).
        if (item != null)
        {
            if (!item.StaleWhileRevalidate && item.FreshSpan.HasValue && item.CreateTime.Add(item.FreshSpan.Value) < DateTime.UtcNow)
            {
                await collection.DeleteOneAsync(x => x.Id == item.Id, OneOption<CacheEntity>.SingleOrDefault); //TODO: Here we should not need a predicate
                return null;
            }

            return new CacheItem<T>()
            {
                CreateTime = item.CreateTime,
                Data = JsonSerializer.Deserialize<T>(item.Data),
                FreshSpan = item.FreshSpan,
                UpdateTime = item.UpdateTime,
            };
        }

        return null;
    }

    public async Task SetAsync<T>(Key key, CacheItem<T> cacheItem, bool staleWhileRevalidate)
    {
        var item = JsonSerializer.Serialize(cacheItem);
        if (Debugger.IsAttached)
        {
            var convertedBack = JsonSerializer.Deserialize<CacheItem<T>>(item);
            var itemAgain = JsonSerializer.Serialize(convertedBack);
            if (itemAgain != item) throw new InvalidOperationException("Failed to serialize/deserialize back to same result.");
        }

        var entity = new CacheEntity
        {
            Id = key,
            Type = typeof(T).AssemblyQualifiedName,
            Data = JsonSerializer.Serialize(cacheItem.Data),
            CreateTime = cacheItem.CreateTime,
            FreshSpan = cacheItem.FreshSpan,
            UpdateTime = cacheItem.UpdateTime,
            StaleWhileRevalidate = staleWhileRevalidate
        };

        var collection = GetCollection();
        await collection.AddOrReplaceAsync(entity);
    }

    private ICacheRepositoryCollection GetCollection()
    {
        var databaseContext = new DatabaseContext
        {
            CollectionName = _options.CollectionName,
            ConfigurationName = _options.ConfigurationName,
            //DatabasePart = _options.DatabasePart,
        };
        var collection = _collectionProvider.GetCollection<ICacheRepositoryCollection, CacheEntity, string>(databaseContext);
        return collection;
    }

    public Task<bool> BuyMoreTime<T>(Key key)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Invalidate<T>(Key key)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DropAsync(Key key)
    {
        throw new NotImplementedException();
    }

    //private async Task<(ConnectionMultiplexer Multiplexer, string Message)> GetConnection(Type type)
    //{
    //    if (_redisConnection?.IsConnected ?? false) return (_redisConnection, "Connected (Cached).");

    //    var connectionString = _options.ConnectionStringLoader(_serviceProvider, type);
    //    if (string.IsNullOrEmpty(connectionString))
    //    {
    //        if (!_hostEnvironment.IsDevelopment()) _logger?.LogWarning("No connection string set for distributed cache.");
    //        return (null, "No connection string.");
    //    }
    //    if (string.Equals(connectionString, "DISABLED", StringComparison.InvariantCultureIgnoreCase)) return (null, "Disabled.");

    //    try
    //    {
    //        _redisConnection = await ConnectionMultiplexer.ConnectAsync(connectionString);
    //        return (_redisConnection, "Connected to Redis.");
    //    }
    //    catch (Exception e)
    //    {
    //        _logger?.LogError(e, e.Message);
    //        if (_redisConnection != null) await _redisConnection.DisposeAsync();
    //        _redisConnection = null;
    //        return (null, e.Message);
    //    }
    //}

    public void Dispose()
    {
        // TODO release managed resources here
    }

    public async ValueTask DisposeAsync()
    {
        // TODO release managed resources here
    }
}