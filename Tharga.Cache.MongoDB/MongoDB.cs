using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;
using MongoDB.Driver;
using Tharga.MongoDB;

namespace Tharga.Cache.MongoDB;

internal class MongoDB : IMongoDB
{
    private readonly ILogger<MongoDB> _logger;
    private readonly ICollectionProvider _collectionProvider;
    private readonly MongoDBCacheOptions _options;

    public MongoDB(ICollectionProvider collectionProvider, IOptions<MongoDBCacheOptions> options, ILogger<MongoDB> logger)
    {
        _collectionProvider = collectionProvider;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<CacheItem<T>> GetAsync<T>(Key key)
    {
        var collection = GetCollection();

        var item = await collection.GetOneAsync(x => x.Id == key.Value, OneOption<CacheEntity>.SingleOrDefault); //TODO: Should be possible to provide option with ID (not predicate).
        if (item != null)
        {
            if (!item.StaleWhileRevalidate && item.FreshSpan.HasValue && item.FreshSpan.Value != TimeSpan.MaxValue && item.CreateTime.Add(item.FreshSpan.Value) < DateTime.UtcNow)
            {
                await collection.DeleteOneAsync(x => x.Id == item.Id, OneOption<CacheEntity>.SingleOrDefault); //TODO: Here we should not need a predicate
                return null;
            }

            return new CacheItem<T>()
            {
                KeyParts = key.KeyParts,
                CreateTime = item.CreateTime,
                Data = JsonSerializer.Deserialize<T>(item.Data),
                FreshSpan = item.FreshSpan,
                UpdateTime = item.UpdateTime,
            };
        }

        return null;
    }

    public IAsyncEnumerable<(Key Key, CacheItem<T> CacheItem)> FindAsync<T>(Key key)
    {
        throw new NotImplementedException();
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

    public Task<bool> BuyMoreTime<T>(Key key)
    {
        return SetUpdateTime(key, DateTime.UtcNow);
    }

    public Task<bool> Invalidate<T>(Key key)
    {
        return SetUpdateTime(key, DateTime.MinValue);
    }

    public async Task<bool> DropAsync<T>(Key key)
    {
        var collection = GetCollection();
        var item = await collection.DeleteOneAsync(x => x.Id == key.Value, OneOption<CacheEntity>.SingleOrDefault); //TODO: Here we should not need a predicate
        return item != null;
    }

    private async Task<bool> SetUpdateTime(Key key, DateTime updateTime)
    {
        var collection = GetCollection();
        var update = new UpdateDefinitionBuilder<CacheEntity>().Set(x => x.UpdateTime, updateTime);
        var result = await collection.UpdateOneAsync(key.Value, update, OneOption<CacheEntity>.SingleOrDefault);
        return result.Before != null;
    }

    public async Task<(bool Success, string Message)> CanConnectAsync()
    {
        try
        {
            var collection = GetCollection();
            var cnt = await collection.CountAsync(x => true);
            return (true, $"There {(cnt == 1 ? "is" : "are")} {cnt} record{(cnt == 1 ? "" : "s")} cached.");
        }
        catch (Exception e)
        {
            _logger?.LogError(e, e.Message);
            return (false, e.Message);
        }
    }

    private ICacheRepositoryCollection GetCollection()
    {
        var databaseContext = new DatabaseContext
        {
            CollectionName = _options.CollectionName,
            ConfigurationName = _options.ConfigurationName
        };
        var collection = _collectionProvider.GetCollection<ICacheRepositoryCollection, CacheEntity, string>(databaseContext);
        return collection;
    }

    public void Dispose()
    {
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}