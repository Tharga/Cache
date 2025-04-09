using Quilt4Net.Toolkit;
using Quilt4Net.Toolkit.Features.Health;

namespace Tharga.Cache.Web;

public class CacheMonitorBackgroundService : BackgroundService
{
    private readonly ICacheMonitor _cacheMonitor;
    private readonly ITimeToLiveCache _ttlCache;
    private readonly ILogger<CacheMonitorBackgroundService> _logger;

    public CacheMonitorBackgroundService(ICacheMonitor cacheMonitor, ITimeToLiveCache ttlCache, ILogger<CacheMonitorBackgroundService> logger)
    {
        _cacheMonitor = cacheMonitor;
        _ttlCache = ttlCache;
        _logger = logger;
        _ttlCache.DataDropEvent += (s, e) =>
        {
            logger.LogInformation($"Data with key {e.Key} was dropped.");
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var infos = _cacheMonitor.GetInfos();
            var response = infos.Select(x => new { Type = x.Type.Name, ItemCount = x.Items.Count, Size = x.Items.Sum(y => y.Value.Size) });
            var ln = response.FirstOrDefault();
            if (ln != null)
            {
                _logger.LogInformation($"{ln.Type}: Count: {ln.ItemCount}, Size: {ln.Size}");
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}

internal class ComponentService : IComponentService
{
    private readonly ICacheMonitor _cacheMonitor;

    public ComponentService(ICacheMonitor cacheMonitor)
    {
        _cacheMonitor = cacheMonitor;
    }

    public IEnumerable<Component> GetComponents()
    {
        yield return new Component
        {
            Name = "DistributedCache",
            Essential = false,
            CheckAsync = _ =>
            {
                var result = _cacheMonitor.GetHealthAsync();
                return Task.FromResult(new CheckResult
                {
                    Success = result.Success,
                    Message = result.Message
                });
            }
        };
    }
}