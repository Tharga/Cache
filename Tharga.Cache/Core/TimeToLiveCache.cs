using Microsoft.Extensions.Logging;

namespace Tharga.Cache.Core;

internal class TimeToLiveCache : TimeCacheBase, ITimeToLiveCache
{
    public TimeToLiveCache(IManagedCacheMonitor cacheMonitor, IPersistLoader persistLoader, IFetchQueue fetchQueue, CacheOptions options, ILogger logger = null)
        : base(cacheMonitor, persistLoader, fetchQueue, options, logger)
    {
    }
}