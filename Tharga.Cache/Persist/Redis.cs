using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Tharga.Cache.Persist;

internal class Redis : IRedis
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
            var cacheItem = JsonSerializer.Deserialize<CacheItem<T>>(data);
            return cacheItem;
        }

        return default;
    }

    public async Task SetAsync<T>(Key key, T data, TimeSpan? freshSpan, bool staleWhileRevalidate)
    {
        var cacheItem = CacheItemBuilder.BuildCacheItem(data, freshSpan);
        var item = JsonSerializer.Serialize(cacheItem);
        if (Debugger.IsAttached)
        {
            var convertedBack = JsonSerializer.Deserialize<CacheItem<T>>(item);
            var itemAgain = JsonSerializer.Serialize(convertedBack);
            if (itemAgain != item) throw new InvalidOperationException("Failed to serialize/deserialize back to same result.");
        }

        var redisConnection = await GetConnection();
        if (redisConnection == default) return;

        var db = redisConnection.GetDatabase();
        if (freshSpan == null || freshSpan == TimeSpan.MaxValue || staleWhileRevalidate)
            await db.StringSetAsync((string)key, item);
        else
            await db.StringSetAsync((string)key, item, freshSpan);
    }

    public async Task<bool> DropAsync<T>(Key key)
    {
        var redisConnection = await GetConnection();
        if (redisConnection == default) return default;

        var db = redisConnection.GetDatabase();
        return await db.KeyDeleteAsync((string)key);
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