using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Tharga.Cache.Persist;

internal class Redis : IRedis, IAsyncDisposable, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<Redis> _logger;
    private readonly CacheOptions _options;
    private ConnectionMultiplexer _redisConnection;

    public Redis(IServiceProvider serviceProvider, IHostEnvironment hostEnvironment, IOptions<CacheOptions> options, ILogger<Redis> logger)
    {
        _serviceProvider = serviceProvider;
        _hostEnvironment = hostEnvironment;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<CacheItem<T>> GetAsync<T>(Key key)
    {
        var redisConnection = await GetConnection();
        if (redisConnection == default) return default;

        var db = redisConnection.GetDatabase();
        var data = await db.StringGetAsync((string)key);
        if (!string.IsNullOrEmpty(data))
        {
            return JsonSerializer.Deserialize<CacheItem<T>>(data);
        }

        return default;
    }

    public async Task SetAsync<T>(Key key, T data, TimeSpan? freshSpan)
    {
        var cacheItem = CacheItemBuilder.BuildCacheItem(data, freshSpan);
        var item = JsonSerializer.Serialize(cacheItem);
        if (Debugger.IsAttached)
        {
            var confirm = JsonSerializer.Deserialize<CacheItem<T>>(item);
            var item2 = JsonSerializer.Serialize(confirm);
            if (item != item2) Debugger.Break();
        }

        var redisConnection = await GetConnection();
        if (redisConnection == default) return;

        var db = redisConnection.GetDatabase();
        if (freshSpan == null || freshSpan == TimeSpan.MaxValue)
            await db.StringSetAsync((string)key, item);
        else
            await db.StringSetAsync((string)key, item, freshSpan);
    }

    public Task<CacheItem<T>> DropAsync<T>(Key key)
    {
        throw new NotImplementedException();
    }

    public IAsyncEnumerable<CacheItem<T>> GetAsync<T>()
    {
        throw new NotImplementedException();
    }

    public Task<(Key Key, CacheItem<T> Item)> DropFirst<T>()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        _redisConnection?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_redisConnection != null) await _redisConnection.DisposeAsync();
    }

    //public Task<CacheItem> DropAsync<T>(Key key)
    //{
    //    throw new NotImplementedException();
    //}

    //public IAsyncEnumerable<CacheItem> GetAsync<T>()
    //{
    //    throw new NotImplementedException();
    //}

    //public Task<(Key Key, CacheItem Item)> DropFirst()
    //{
    //    throw new NotImplementedException();
    //}

    private async Task<ConnectionMultiplexer> GetConnection()
    {
        if (_redisConnection?.IsConnected ?? false) return _redisConnection;

        var connectionString = _options.ConnectionStringLoader(_serviceProvider);
        if (string.IsNullOrEmpty(connectionString))
        {
            if (_hostEnvironment.IsDevelopment()) return default;
            _logger?.LogWarning("No connection string set for distributed cache.");
            return default;
        }
        if (string.Equals(connectionString, "DISABLED", StringComparison.InvariantCultureIgnoreCase)) return default;

        try
        {
            _redisConnection = await ConnectionMultiplexer.ConnectAsync(connectionString);
            return _redisConnection;
        }
        catch (Exception e)
        {
            _logger?.LogError(e, e.Message);
            if (_redisConnection != null) await _redisConnection.DisposeAsync();
            _redisConnection = null;
            return default;
        }
    }
}

/*
builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();

public class RedisCacheService : IRedisCacheService
   {
       private readonly IDatabase _db;

       public RedisCacheService(IConnectionMultiplexer redis)
       {
           _db = redis.GetDatabase();
       }

       public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
       {
           var json = JsonSerializer.Serialize(value);
           await _db.StringSetAsync(key, json, expiration);
       }

       public async Task<T?> GetAsync<T>(string key)
       {
           var value = await _db.StringGetAsync(key);
           return value.HasValue
               ? JsonSerializer.Deserialize<T>(value!)
               : default;
       }

       public async Task RemoveAsync(string key)
       {
           await _db.KeyDeleteAsync(key);
       }
   }

 */

internal class Memory : IMemory
{
    private readonly ConcurrentDictionary<string, CacheItem> _datas = new();

    public async Task<CacheItem<T>> GetAsync<T>(Key key)
    {
        return (CacheItem<T>)_datas.GetValueOrDefault(key);
    }

    public Task SetAsync<T>(Key key, T data, TimeSpan? freshSpan)
    {
        var item = CacheItemBuilder.BuildCacheItem(data, freshSpan);
        _datas.AddOrUpdate(key, item, (_, _) => item);
        return Task.CompletedTask;
    }

    public async Task<CacheItem<T>> DropAsync<T>(Key key)
    {
        if (_datas.TryRemove(key, out var val)) return (CacheItem<T>)val;
        return null;
    }

    public IAsyncEnumerable<CacheItem<T>> GetAsync<T>()
    {
        throw new NotImplementedException();
    }

    public async Task<(Key Key, CacheItem<T> Item)> DropFirst<T>()
    {
        var item = _datas.OrderBy(x => x.Value.CreateTime).First();
        _datas.TryRemove(item.Key, out var removed);
        return (item.Key, (CacheItem<T>)removed);
    }
}

internal static class CacheItemBuilder
{
    public static CacheItem BuildCacheItem<T>(T data, TimeSpan? freshSpan)
    {
        return new CacheItem<T> { Data = data, FreshSpan = freshSpan, CreateTime = DateTime.UtcNow };
    }
}