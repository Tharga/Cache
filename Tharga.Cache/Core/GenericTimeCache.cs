using Microsoft.Extensions.Logging;

namespace Tharga.Cache.Core;

internal class GenericTimeCache : TimeCacheBase
{
    public GenericTimeCache(IManagedCacheMonitor cacheMonitor, IPersistLoader persistLoader, IFetchQueue fetchQueue, CacheOptions options, ILogger logger = null)
        : base(cacheMonitor, persistLoader, fetchQueue, options, logger)
    {
    }
}