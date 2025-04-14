using Tharga.Console.Commands.Base;

namespace Tharga.Cache.Console.Commands;

internal class CacheFecthQueueCommand : AsyncActionCommandBase
{
    private readonly ICacheMonitor _cacheMonitor;

    public CacheFecthQueueCommand(ICacheMonitor cacheMonitor) 
        : base("fetchqueue")
    {
        _cacheMonitor = cacheMonitor;
    }

    public override async Task InvokeAsync(string[] param)
    {
        OutputInformation($"Queue count: {_cacheMonitor.GetFetchQueueCount()}");
    }
}

internal class CacheInvalidateCommand : AsyncActionCommandBase
{
    private readonly ITimeToLiveCache _timeToLiveCache;

    public CacheInvalidateCommand(ITimeToLiveCache timeToLiveCache)
        : base("invalidate")
    {
        _timeToLiveCache = timeToLiveCache;
    }

    public override async Task InvokeAsync(string[] param)
    {
        var result = await _timeToLiveCache.InvalidateAsync<string>("key");
        OutputInformation($"Invalidated {result}");
    }
}