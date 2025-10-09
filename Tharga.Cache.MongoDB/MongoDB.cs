using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Tharga.MongoDB;

namespace Tharga.Cache.MongoDB;

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
            ConfigurationName = _options.ConfigurationName
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

    public void Dispose()
    {
    }

    public async ValueTask DisposeAsync()
    {
    }
}