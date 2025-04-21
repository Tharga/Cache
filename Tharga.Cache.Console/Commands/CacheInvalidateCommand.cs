using Tharga.Console.Commands.Base;

namespace Tharga.Cache.Console.Commands;

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