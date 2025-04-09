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
- Make sure dual calls does not make more database calls
- Queue loaders so they only run in x number of parallels
- Health check

Cache Topology / Strategy
- Write-through
- Write-behind
- Cache-aside
- Read-through