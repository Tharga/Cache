using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Tharga.Cache.Core;

namespace Tharga.Cache.Persist;

internal class Redis : IRedis
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<Redis> _logger;
    private readonly CacheOptions _options;
    private ConnectionMultiplexer _redisConnection;

    public Redis(IServiceProvider serviceProvider, IHostEnvironment hostEnvironment, IManagedCacheMonitor cacheMonitor, IOptions<CacheOptions> options, ILogger<Redis> logger)
    {
        _serviceProvider = serviceProvider;
        _hostEnvironment = hostEnvironment;
        _logger = logger;
        _options = options.Value;

        cacheMonitor.RequestEvictEvent += (s, e) =>
        {
            //TODO: Implement
            Debugger.Break();
            throw new NotImplementedException();
        };
    }

    public async Task<CacheItem<T>> GetAsync<T>(Key key)
    {
        var redisConnection = await GetConnection();
        if (redisConnection.Multiplexer == default) return default;

        var db = redisConnection.Multiplexer.GetDatabase();
        var data = await db.StringGetAsync((string)key);
        if (!string.IsNullOrEmpty(data))
        {
            var cacheItem = JsonSerializer.Deserialize<CacheItem<T>>(data);
            return cacheItem;
        }

        return default;
    }

    public async Task SetAsync<T>(Key key, CacheItem<T> cacheItem, bool staleWhileRevalidate)
    {
        //var cacheItem = CacheItemBuilder.BuildCacheItem(data, freshSpan);
        var item = JsonSerializer.Serialize(cacheItem);
        if (Debugger.IsAttached)
        {
            var convertedBack = JsonSerializer.Deserialize<CacheItem<T>>(item);
            var itemAgain = JsonSerializer.Serialize(convertedBack);
            if (itemAgain != item) throw new InvalidOperationException("Failed to serialize/deserialize back to same result.");
        }

        var redisConnection = await GetConnection();
        if (redisConnection.Multiplexer == default) return;

        var db = redisConnection.Multiplexer.GetDatabase();
        if (cacheItem.FreshSpan == null || cacheItem.FreshSpan == TimeSpan.MaxValue || staleWhileRevalidate)
            await db.StringSetAsync((string)key, item);
        else
            await db.StringSetAsync((string)key, item, cacheItem.FreshSpan);
    }

    public async Task<bool> BuyMoreTime<T>(Key key)
    {
        return await SetUpdateTime<T>(key, DateTime.UtcNow);
    }

    public async Task<bool> Invalidate<T>(Key key)
    {
        return await SetUpdateTime<T>(key, DateTime.MinValue);
    }

    private async Task<bool> SetUpdateTime<T>(Key key, DateTime updateTime)
    {
        var redisConnection = await GetConnection();
        if (redisConnection.Multiplexer == default) return default;

        var db = redisConnection.Multiplexer.GetDatabase();
        var data = await db.StringGetAsync((string)key);
        if (!string.IsNullOrEmpty(data))
        {
            var cacheItem = JsonSerializer.Deserialize<CacheItem<T>>(data);
            var updatedCacheItem = cacheItem with { UpdateTime = updateTime };
            var item = JsonSerializer.Serialize(updatedCacheItem);
            if (Debugger.IsAttached)
            {
                var convertedBack = JsonSerializer.Deserialize<CacheItem<T>>(item);
                var itemAgain = JsonSerializer.Serialize(convertedBack);
                if (itemAgain != item) throw new InvalidOperationException("Failed to serialize/deserialize back to same result.");
            }
            await db.StringSetAsync((string)key, item);
            return true;
        }

        return false;
    }

    public async Task<bool> DropAsync<T>(Key key)
    {
        var redisConnection = await GetConnection();
        if (redisConnection.Multiplexer == default) return default;

        var db = redisConnection.Multiplexer.GetDatabase();
        return await db.KeyDeleteAsync((string)key);
    }

    public void Dispose()
    {
        _redisConnection?.Dispose();
    }

    public async Task<(bool Success, string Message)> CanConnectAsync()
    {
        var redisConnection = await GetConnection();
        if (redisConnection.Multiplexer == default) return (false, redisConnection.Message);

        return (redisConnection.Multiplexer.IsConnected, redisConnection.Message);
    }

    public async ValueTask DisposeAsync()
    {
        if (_redisConnection != null) await _redisConnection.DisposeAsync();
    }

    private async Task<(ConnectionMultiplexer Multiplexer, string Message)> GetConnection()
    {
        if (_redisConnection?.IsConnected ?? false) return (_redisConnection, "Connected (Cached).");

        var connectionString = _options.ConnectionStringLoader(_serviceProvider);
        if (string.IsNullOrEmpty(connectionString))
        {
            if (!_hostEnvironment.IsDevelopment()) _logger?.LogWarning("No connection string set for distributed cache.");
            return (default, "No connection string.");
        }
        if (string.Equals(connectionString, "DISABLED", StringComparison.InvariantCultureIgnoreCase)) return (default, "Disabled.");

        try
        {
            _redisConnection = await ConnectionMultiplexer.ConnectAsync(connectionString);
            return (_redisConnection, "Connected to Redis.");
        }
        catch (Exception e)
        {
            _logger?.LogError(e, e.Message);
            if (_redisConnection != null) await _redisConnection.DisposeAsync();
            _redisConnection = null;
            return (default, e.Message);
        }
    }
}