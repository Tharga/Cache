using Tharga.Console.Commands.Base;

namespace Tharga.Cache.Console.Commands;

internal class CacheSetCommand : AsyncActionCommandBase
{
    private readonly ITimeToLiveCache _timeToLiveCache;

    public CacheSetCommand(ITimeToLiveCache timeToLiveCache)
        : base("set")
    {
        _timeToLiveCache = timeToLiveCache;
    }

    public override async Task InvokeAsync(string[] param)
    {
        await _timeToLiveCache.SetAsync("key", "abc123");
    }
}