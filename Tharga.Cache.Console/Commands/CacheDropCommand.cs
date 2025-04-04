using Tharga.Console.Commands.Base;

namespace Tharga.Cache.Console.Commands;

internal class CacheDropCommand : AsyncActionCommandBase
{
    private readonly ITimeToLiveCache _timeToLiveCache;

    public CacheDropCommand(ITimeToLiveCache timeToLiveCache)
        : base("drop")
    {
        _timeToLiveCache = timeToLiveCache;
    }

    public override async Task InvokeAsync(string[] param)
    {
        await _timeToLiveCache.DropAsync<string>("key");
    }
}