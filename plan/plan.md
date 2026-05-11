# Plan: Fix AddCache singleton options capture

## Steps

- [x] **1. Write the regression test first (red)**
  Added `AddCache_CalledTwice_SecondCallRegistrationHonoredAtRuntime` to `AddCacheIdempotencyTests.cs`. Calls `AddCache` twice with `RegisterType<string>` then `RegisterType<int>`, both with `DefaultFreshSpan`. Resolves `ITimeToLiveCache`, calls `GetAsync<int>` with no explicit freshSpan, asserts it does not throw.

- [x] **2. Confirmed test fails on master**
  Failed with the exact Eplicta-reported exception: `InvalidOperationException: No freshSpan provided and no DefaultFreshSpan configured for cache type Int32` at `TimeCacheBase.GetDefaultFreshSpan[T]()`. Bug reproduced.

- [x] **3. Fix the 6 factories in `CacheRegistrationExtensions.AddCache`**
  All factories now call `var opts = s.GetRequiredService<IOptions<CacheOptions>>().Value;` and pass `opts` instead of the captured `o`. Fixed: `IFetchQueue`, `IManagedCacheMonitor`, `IEternalCache`, `ITimeToLiveCache`, `ITimeToIdleCache`, `IScopeCache`.

- [x] **4. Run the test suite**
  - `Tharga.Cache.Tests`: 474/474 pass (regression test included, plus one re-run for a flaky time-critical concurrency test unrelated to this fix).
  - `Tharga.Cache.Redis.Tests`: 2/2 pass.
  - `Tharga.Cache.File.Tests`: 2 skipped (file-infra-dependent).
  - `Tharga.Cache.MongoDB.Tests`: no tests discovered (integration-only project).

- [x] **5. Check README for one-call guidance**
  Grepped `README.md` and `Tharga.Cache/README.md` — no text suggesting `AddCache` must be called once. No README change needed.

- [x] **6. Bump version `Tharga.Cache.csproj` 1.0.0 → 1.1.0**
  Done.

- [~] **7. Commit + push**
  Conventional commit: `fix: resolve CacheOptions from DI in singleton factories so multi-call AddCache merges correctly`. Push branch.

- [ ] **8. Open PR `feature/fix-addcache-options-capture` → `master`**

- [ ] **9. Wait for CI green; hand to user**
  Do not merge — PR is the review gate. User to confirm done, then per shared-instructions: archive `plan/feature.md` to `$DOC_ROOT/Tharga/plans/Toolkit/Cache/done/fix-addcache-options-capture.md`, delete `plan/`, mark the Eplicta request `Status: Done` with completion date and add the follow-up entry in `Requests.md` (Eplicta should bump Tharga.Cache to 1.1.0 and remove their workaround in `Core/Eplicta.Aggregator/Program.cs`).

## Notes

- Scope was expanded from the 4 caches Eplicta listed to all 6 factories. `IFetchQueue` (reads `MaxConcurrentFetchCount`) and `IManagedCacheMonitor` (passes `CacheOptions` to `CacheMonitor`) have the same closure-capture bug and were fixed for consistency.
- First-call-wins on type registrations preserved (`AppendPreviousRegistrations` uses `TryAdd`). Only the singleton-options binding changed — singletons now lazily resolve `IOptions<CacheOptions>` from the SP, picking up the merged view from the last `AddCache` call.

## Last session

Implementation complete on `feature/fix-addcache-options-capture`. All 6 factories rewritten, regression test added and passing, version bumped to 1.1.0. Pending: commit, push, open PR, wait for CI, user confirmation, then archive plan + close Eplicta request with follow-up.
