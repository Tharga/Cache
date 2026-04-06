# Feature: monitor-track-persisted

## Goal
Make cache items loaded from persistence (MongoDB, Redis, File) visible in the Blazor monitoring UI on first access, not only after re-fetch.

## Scope
- `IManagedCacheMonitor` — add `Track<T>` method
- `CacheMonitor` — implement `Track<T>` (register in `_caches` without firing `DataSetEvent`)
- `CacheBase.GetCoreAsync` — call `Track<T>` when a fresh item is found from persistence but isn't yet in the monitor
- Tests verifying items are tracked on persistence hit

## Acceptance Criteria
- Fresh items found in persistence appear in `ICacheMonitor.GetInfos()` after first access
- `DataSetEvent` is NOT fired for items loaded from persistence (only `DataGetEvent`)
- All existing tests still pass

## Done Condition
User confirms the fix is satisfactory.

## Originating Branch
develop
