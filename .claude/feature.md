# Feature: eplicta-cache-requests

## Goal
Address two pending feature requests from Eplicta.Core (`c:\dev\Eplicta\.claude\requests.md`).

## Originating Branch
`develop`

## Scope

### Request 1: Deprecate IMemoryWithRedis
Mark `IMemoryWithRedis` and `MemoryWithRedis` as `[Obsolete]` — the type couples two storage concerns and consumers should use `IRedis` or `IMemory` explicitly.

### Request 2: ReturnDefaultOnFirstLoad option
Add a `ReturnDefaultOnFirstLoad` property to `CacheTypeOptions` that returns `default(T)` immediately on the first cache miss instead of blocking. The factory runs in the background. Works independently of `StaleWhileRevalidate`.

## Acceptance Criteria
- [ ] `IMemoryWithRedis` and `MemoryWithRedis` are marked `[Obsolete]`
- [ ] `ReturnDefaultOnFirstLoad` option exists on `CacheTypeOptions`
- [ ] When enabled, first cache miss returns `default(T)` and triggers background load
- [ ] Works independently of `StaleWhileRevalidate` (all 4 combinations valid)
- [ ] Tests cover all scenarios
- [ ] All existing tests pass

## Done Condition
Both requests implemented, all tests pass, user confirms done.
