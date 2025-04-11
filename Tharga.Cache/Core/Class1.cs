using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tharga.Cache.Core;

public interface IWatchDogService
{
    void Start(CancellationToken cancellationToken = default);
}

internal class WatchDogService : BackgroundService, IWatchDogService
{
    private readonly ICacheMonitor _cacheMonitor;
    private readonly ILogger<WatchDogService> _logger;
    private readonly TimeSpan _interval;

    public WatchDogService(ICacheMonitor cacheMonitor, IOptions<CacheOptions> options, ILogger<WatchDogService> logger = default)
    {
        _cacheMonitor = cacheMonitor;
        _logger = logger;
        _interval = options.Value.WatchDogInterval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                //await RunOnceAsync(stoppingToken);
                _cacheMonitor.CleanSale();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Unhandled exception in WatchDogService.");
            }

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    //private Task RunOnceAsync(CancellationToken cancellationToken)
    //{
    //    _cacheMonitor.CleanSale();

    //    var infos = _cacheMonitor.GetInfos().Where(x => !x.StaleWhileRevalidate).ToArray();
    //    foreach (var info in infos)
    //    {
    //        foreach (var item in info.Items.Where(x => x.Value.IsStale))
    //        {
    //            _cacheMonitor.CleanSale();
    //        }
    //    }

    //    return Task.CompletedTask;
    //}

    public void Start(CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            await ExecuteAsync(cancellationToken);
        });
    }
}