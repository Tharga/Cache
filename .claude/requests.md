## Pending

### AddCache should be idempotent (multiple calls must not throw)
- **From:** Florida
- **Date:** 2026-04-05
- **Priority:** High
- **Description:** Calling `AddCache()` more than once throws `System.InvalidOperationException: The type EnvironmentOption[] has already been registered` in `AppendPreviousRegistrations`. This happens when both Platform (`AddThargaTeamBlazor`) and Quilt4Net (`AddQuilt4NetApplicationInsightsClient`) register cache types — both internally call `AddCache`. It also breaks `WebApplicationFactory` integration tests which rebuild the host. Fix: `AddCache` should detect previous registrations and merge rather than throw. Multiple `AddCache` calls should be safe and additive.
- **Status:** Pending

### SetAsync for IMemory-registered types
- **From:** Florida
- **Date:** 2026-04-05
- **Priority:** High
- **Description:** `ITimeToLiveCache.SetAsync` currently only works with `IFile`-registered types (used for file-based persistence). Need `SetAsync<T>(Key key, T value)` to work for `IMemory`-registered types too, so consumers can directly write/update cached values without going through `DropAsync` + `GetAsync` (which forces a full re-fetch from the source). Use case: after saving an article to Fortnox, update the cached article list by adding/replacing the single changed item — avoiding a full reload of 3000+ articles from Fortnox. The value should be stored as fresh with the type's configured `DefaultFreshSpan`.
- **Status:** Pending

### Fast path for fresh cache hits in IMemory
- **From:** Florida
- **Date:** 2026-03-28
- **Priority:** Medium
- **Description:** When a cached item is fresh and already in the ConcurrentDictionary, the current implementation still goes through the semaphore/fetch queue/dispatch loop. For use cases where IMemoryCache-level performance is needed (e.g. caching external API lookups that happen on every request), a fast path that skips the synchronization overhead for fresh hits would be valuable. This would keep the smart features (StaleWhileRevalidate, background refresh, eviction) but make cache hits nearly as fast as a raw dictionary lookup.
- **Status:** Pending
