# Feature: cache-load-time

## Goal
Track how long the fetch delegate takes and display it in the Blazor monitoring UI.

## Scope
- `CacheItem` — add `LoadDuration` field
- `CacheItemBuilder` — accept and store `LoadDuration`
- `FetchQueue` — measure fetch time with `Stopwatch`
- `CacheItemInfo` — add `LoadDuration` property, remove TODO comment
- `CacheMonitor.Set/Track` — pass `LoadDuration` to `CacheItemInfo`
- `IManagedCacheMonitor` — update `Set`/`Track` signatures
- `CacheBase` — pass `LoadDuration` through `OnSetAsync` and `TrackIfNeeded`
- `ListView.razor` — add "Load Time" column

## Acceptance Criteria
- `CacheItemInfo.LoadDuration` reflects the actual fetch delegate execution time
- Blazor ListView shows load duration for each cached item
- Items loaded from persistence (Track path) show null load duration
- All existing tests still pass

## Done Condition
User confirms the feature is satisfactory.

## Originating Branch
develop
