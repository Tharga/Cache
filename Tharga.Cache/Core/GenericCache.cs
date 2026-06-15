using Microsoft.Extensions.Logging;

namespace Tharga.Cache.Core;

internal class GenericCache : CacheBase
{
    public GenericCache(IManagedCacheMonitor cacheMonitor, IPersistLoader persistLoader, IFetchQueue fetchQueue, CacheOptions options, ILogger logger = null)
        : base(cacheMonitor, persistLoader, fetchQueue, options, logger)
    {
    }

    protected override TimeSpan GetDefaultFreshSpan<T>()
    {
        throw new NotImplementedException();
    }
}