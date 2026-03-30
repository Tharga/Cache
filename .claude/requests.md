## Pending

### Fast path for fresh cache hits in IMemory
- **From:** Florida (`c:\dev\tharga\Florida`)
- **Date:** 2026-03-28
- **Priority:** Medium
- **Description:** When a cached item is fresh and already in the ConcurrentDictionary, the current implementation still goes through the semaphore/fetch queue/dispatch loop. For use cases where IMemoryCache-level performance is needed (e.g. caching external API lookups that happen on every request), a fast path that skips the synchronization overhead for fresh hits would be valuable. This would keep the smart features (StaleWhileRevalidate, background refresh, eviction) but make cache hits nearly as fast as a raw dictionary lookup.
- **Status:** Pending
