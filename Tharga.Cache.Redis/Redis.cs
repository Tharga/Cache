using System.Diagnostics;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using StackExchange.Redis;
using Tharga.Cache.Core;

namespace Tharga.Cache.Redis;

internal class Redis : IRedis
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly CacheOptions _options;
    private ConnectionMultiplexer _redisConnection;
    private readonly ILogger<Redis> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public Redis(IServiceProvider serviceProvider, IHostEnvironment hostEnvironment, IManagedCacheMonitor cacheMonitor, IOptions<CacheOptions> options, ILogger<Redis> logger)
    {
        _serviceProvider = serviceProvider;
        _hostEnvironment = hostEnvironment;
        _options = options.Value;
        _logger = logger;
        _retryPolicy = Policy
            .Handle<RedisException>()
            .Or<TimeoutException>()
            .Or<SocketException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt)),
                onRetry: (exception, timeSpan, retryCount, _) =>
                {
                    _logger.LogWarning($"Retry {retryCount} after {timeSpan.TotalMilliseconds}ms due to: {exception.Message}");
                });

        cacheMonitor.RequestEvictEvent += async (_, e) =>
        {
            await DropAsync(e.Key);
            cacheMonitor.Drop(e.Type, e.Key);
        };
    }

    public async Task<CacheItem<T>> GetAsync<T>(Key key)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var redisConnection = await GetConnection(typeof(IRedis));
            if (redisConnection.Multiplexer == null) return null;

            var db = redisConnection.Multiplexer.GetDatabase();
            var data = await db.StringGetAsync((string)key);
            if (!string.IsNullOrEmpty(data))
            {
                var cacheItem = JsonSerializer.Deserialize<CacheItem<T>>(data);
                return cacheItem;
            }

            return null;
        });
    }

    public async Task SetAsync<T>(Key key, CacheItem<T> cacheItem, bool staleWhileRevalidate)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            var item = JsonSerializer.Serialize(cacheItem);
            if (Debugger.IsAttached)
            {
                var convertedBack = JsonSerializer.Deserialize<CacheItem<T>>(item);
                var itemAgain = JsonSerializer.Serialize(convertedBack);
                if (itemAgain != item) throw new InvalidOperationException("Failed to serialize/deserialize back to same result.");
            }

            var redisConnection = await GetConnection(typeof(IRedis));
            if (redisConnection.Multiplexer == null) return;

            var db = redisConnection.Multiplexer.GetDatabase();
            if (cacheItem.FreshSpan == null || cacheItem.FreshSpan == TimeSpan.MaxValue || staleWhileRevalidate)
                await db.StringSetAsync((string)key, item);
            else
                await db.StringSetAsync((string)key, item, cacheItem.FreshSpan);
        });
    }

    public async Task<bool> BuyMoreTime<T>(Key key)
    {
        return await _retryPolicy.ExecuteAsync(async () => await SetUpdateTime<T>(key, DateTime.UtcNow));
    }

    public async Task<bool> Invalidate<T>(Key key)
    {
        return await _retryPolicy.ExecuteAsync(async () => await SetUpdateTime<T>(key, DateTime.MinValue));
    }

    public async Task<bool> DropAsync(Key key)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var redisConnection = await GetConnection(typeof(IRedis));
            if (redisConnection.Multiplexer == null) return false;

            var db = redisConnection.Multiplexer.GetDatabase();
            var result = await db.KeyDeleteAsync((string)key);
            return result;
        });
    }

    public async Task<(bool Success, string Message)> CanConnectAsync()
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var redisConnection = await GetConnection(typeof(IRedis));
            if (redisConnection.Multiplexer == null) return (false, redisConnection.Message);

            return (redisConnection.Multiplexer.IsConnected, redisConnection.Message);
        });
    }

    private async Task<bool> SetUpdateTime<T>(Key key, DateTime updateTime)
    {
        var redisConnection = await GetConnection(typeof(IRedis));
        if (redisConnection.Multiplexer == null) return false;

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

    private async Task<(ConnectionMultiplexer Multiplexer, string Message)> GetConnection(Type type)
    {
        if (_redisConnection?.IsConnected ?? false) return (_redisConnection, "Connected (Cached).");

        //var connectionString = _options.ConnectionStringLoader(_serviceProvider, type);
        //if (string.IsNullOrEmpty(connectionString))
        //{
        //    if (!_hostEnvironment.IsDevelopment()) _logger?.LogWarning("No connection string set for distributed cache.");
        //    return (null, "No connection string.");
        //}
        //if (string.Equals(connectionString, "DISABLED", StringComparison.InvariantCultureIgnoreCase)) return (null, "Disabled.");

        //try
        //{
        //    _redisConnection = await ConnectionMultiplexer.ConnectAsync(connectionString);
        //    return (_redisConnection, "Connected to Redis.");
        //}
        //catch (Exception e)
        //{
        //    _logger?.LogError(e, e.Message);
        //    if (_redisConnection != null) await _redisConnection.DisposeAsync();
        //    _redisConnection = null;
        //    return (null, e.Message);
        //}
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
}