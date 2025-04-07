# Tharga Cache

- Eternal
- ScopeCache
- TimeToLive
- TimeToIdle

- ITimeCache (with ExpirationPolicies and PersistType)
- ICache (with ExpirationPolicies and PersistType)

- CacheMonitor
- EvictionPolicy

## TODO
- Clean out when cache is too large
- Health check
- Add distribute provider
- Combine distribute provider with memory cache
- TTI
- Opt-out from loading even if data is stale
- Cascade-load when loading much data at the same time

- Test for distributed "LOCAL"
- Test for distributed "DISABLED"
- Test for distributed (on different instances)
- Test local (different instances) ==> Should be isolated.
- Tests for background tasks that should load values in the background, trigger an event and then be updated.
- Tests for rolling cache, should be keept in memory as long as it is used.

- Make sure dual calls does not make more database calls
- Make it possible to opt out from refresh-calls in the background-cache. (Just load what is stored)
- Make it possible to throttle caches (If I have 1000-caches and all tries to load at the same time, especially for the background cahce version)

- Size of cache should increase when adding and decrease when removing
- Combinde local and distributed cache for speed.


//public interface IRedis : IPersist
//{
//}
//public interface IMongoDB : IPersist
//{
//}

//internal class Redis : IRedis
//{
//    /*
//     * - MemoryCache (System.Runtime.Caching)
//     * - MemoryCache based on Microsoft.Extensions.Caching.Memory
//     * - Redis using StackExchange.Redis
//     */

//    /*
//     * Cache Topology / Strategy
//     * - Write-through
//     * - Write-behind
//     * - Cache-aside
//     * - Read-through
//     */

//    /*
//     * Admission Policies
//     * - Bloom Filter
//     * - Doorkeeper
//     */
//    public Task<(bool, T)> GetAsync<T>(Key key)
//    {
//        throw new NotImplementedException();
//    }

//    public Task SetAsync<T>(Key key, T data)
//    {
//        throw new NotImplementedException();
//    }
//}
//internal class MongoDB : IMongoDB
//{
//    public Task<(bool, T)> GetAsync<T>(Key key)
//    {
//        throw new NotImplementedException();
//    }

//    public Task SetAsync<T>(Key key, T data)
//    {
//        throw new NotImplementedException();
//    }
//}