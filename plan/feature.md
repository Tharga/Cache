# Feature: Fix AddCache singleton options capture

## Goal

`AddCache` is intended to support being called multiple times — its own `AppendPreviousRegistrations` helper plus the `RemoveAll<IOptions<CacheOptions>>` + re-add pattern documents that intent. In practice the second call's registrations are silently lost because the singleton factories close over the local `CacheOptions o` instead of resolving `IOptions<CacheOptions>` from the service provider. The first call's `o` is the one bound to every singleton, regardless of what later calls register.

Switch every affected factory to resolve `IOptions<CacheOptions>` from the SP. Singletons are constructed lazily on first resolution, after composition is complete, so they pick up the final merged options.

## Source

Eplicta request (2026-04-14, High) in `$DOC_ROOT/Tharga/Requests.md`. Reproduces in any app that combines its own `AddCache` with `AddQuilt4NetApplicationInsightsClient`, which calls `AddCache` transitively. Eplicta observes 23 production exceptions on `EnvironmentOption[]`: `InvalidOperationException: "No freshSpan provided and no DefaultFreshSpan configured for cache type ..."` even though the type *was* registered in the second call — just not in the `o` the first call's closure captured.

## Scope

In `Tharga.Cache/CacheRegistrationExtensions.cs`, the 6 factories that close over `o`:

1. `IFetchQueue` (line 35) — uses `o.MaxConcurrentFetchCount`
2. `IManagedCacheMonitor` (line 42) — passes `o` to `CacheMonitor`
3. `IEternalCache` (line 60)
4. `ITimeToLiveCache` (line 67)
5. `ITimeToIdleCache` (line 74)
6. `IScopeCache` (line 81) — scoped, but same closure bug applies

Note: the Eplicta request only enumerated the 4 cache types. The two infrastructure singletons (`IFetchQueue`, `IManagedCacheMonitor`) have the same closure-capture bug and need the same fix for consistency. Without this they would still see first-call options if `MaxConcurrentFetchCount` / `WatchDogInterval` ever change between calls.

Out of scope:
- Public API changes
- `RegisterAllIPersistImplementations`, `InvokeAllPersistRegistrations` — these run eagerly during `AddCache` itself, so reading the local `o` is correct.
- `o.RegisterConfigurations(serviceCollection)` callbacks — eager, local `o` is correct.
- Tharga.Console 3.7.4 → 4.0.0 upgrade in `Tharga.Cache.Console` (deferred to a separate change).

## Acceptance criteria

- All 6 factories resolve `IOptions<CacheOptions>` from the SP. No factory closes over the local `o`.
- New regression test in `AddCacheIdempotencyTests.cs`: call `AddCache` twice with different `RegisterType<>` calls, build the SP, resolve a cache, and prove the **second** call's registrations are honored (e.g. `DefaultFreshSpan` set in the second call works at runtime).
- All existing tests in `Tharga.Cache.Tests` still pass.
- `Tharga.Cache.csproj` version bumped: `1.0.0` → `1.1.0` (minor; user-chosen).
- README updated only if it documents one-call-only registration (check `Tharga.Cache/README.md` for guidance about `AddCache` call count).

## Done condition

- Feature branch `feature/fix-addcache-options-capture` pushed
- PR opened from `feature/fix-addcache-options-capture` → `master` (GitHub Actions branching strategy)
- CI green
- User confirms the fix as done
- Eplicta workaround referenced in `Core/Eplicta.Aggregator/Program.cs` (per the Requests.md note) can be removed once the new `Tharga.Cache` package version is consumed — tracked as follow-up in `Requests.md`
