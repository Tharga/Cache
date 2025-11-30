using Tharga.Console.Commands.Base;

namespace Tharga.Cache.Console.Commands;

internal class CachePeekCommand : AsyncActionCommandBase
{
    private readonly ITimeToLiveCache _timeToLiveCache;

    public CachePeekCommand(ITimeToLiveCache timeToLiveCache)
        : base("peek")
    {
        _timeToLiveCache = timeToLiveCache;
    }

    public override async Task InvokeAsync(string[] param)
    {
        var item = await _timeToLiveCache.PeekAsync<string>("key");
        OutputInformation(item);
    }
}