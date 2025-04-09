using System.Diagnostics;
using Tharga.Console.Commands.Base;

namespace Tharga.Cache.Console.Commands;

internal class CacheGetCommand : AsyncActionCommandBase
{
    private readonly ITimeToLiveCache _timeToLiveCache;

    public CacheGetCommand(ITimeToLiveCache timeToLiveCache)
        : base("get")
    {
        _timeToLiveCache = timeToLiveCache;
    }

    public override async Task InvokeAsync(string[] param)
    {
        var item = await _timeToLiveCache.GetAsync("key", () => Task.FromResult("qwerty"));
        OutputInformation(item);
    }
}