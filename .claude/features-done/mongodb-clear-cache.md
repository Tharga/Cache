# Feature: mongodb-clear-cache

## Goal
Fix ClearCache/ClearStale for MongoDB-backed cache types by subscribing to `RequestEvictEvent`.

## Scope
- `MongoDB.cs` constructor — add `IManagedCacheMonitor` parameter and `RequestEvictEvent` subscription

## Acceptance Criteria
- `ICacheMonitor.ClearAll()` removes MongoDB-persisted items
- `ICacheMonitor.ClearStale()` removes stale MongoDB-persisted items
- Existing tests still pass

## Done Condition
User confirms the fix is satisfactory.

## Originating Branch
develop
