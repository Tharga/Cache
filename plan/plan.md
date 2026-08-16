# Plan

## Steps

- [x] 1. **NuGet updates up front (whole solution).**
      Available: `Tharga.Blazor` 2.3.0 → 2.3.1 (patch); `xunit.v3` 3.2.2 → 4.0.0 and
      `xunit.runner.visualstudio` 3.1.5 → 4.0.0 (major) across the four test projects.
      Apply, then verify build + full suite before any fix code is written.
      Note: `dotnet outdated` is not installed on this machine; used
      `dotnet list package --outdated` instead.

- [x] 1b. **Microsoft.Testing.Platform migration** (forced by the xunit v4 major).
      xunit.v3 4.0.0 drops the VSTest bridge on the .NET 10 SDK — `dotnet test` fails
      with *"Testing with VSTest target is no longer supported"* until the project opts
      into MTP. User chose (2026-08-15) to do the full migration in this PR rather than
      defer it. Requires: `global.json` runner opt-in, dropping the VSTest-only
      packages, translating the CI `--filter` trait expression to xunit's MTP filter
      options, and replacing `--collect:"XPlat Code Coverage"` with an MTP coverage
      route that still emits cobertura into `./coverage` for Codecov.

- [x] 2. **Baseline the suite on the branch.** Backlog item 9 records
      `FetchDataThrottleTests` as failing most runs on unmodified master. Establish
      how it behaves here so a later failure is not misread as caused by the fixes.

- [x] 3. **gh#56 — remove the FluentAssertions reference** from
      `Tharga.Cache/Tharga.Cache.csproj`. Verify the packed nuspec no longer lists it.

- [x] 4. **gh#55 — write the failing test first.** Concurrent `AddCache` across many
      independent `ServiceCollection`s: assert no throw, and assert no registration
      bleeds from one collection into another.

- [x] 5. **gh#55 — implement.** Replace the static dictionary with a merge that reads
      the previous `CacheOptions` off the existing `IOptions<CacheOptions>` descriptor.
      Delete `ResetRegistrations` and the test hooks that call it.

- [x] 6. **Full verification.** Release build + full suite, repeated runs to separate
      real regressions from the known throttle-test flakiness.

- [x] 7. **Documentation review.** Check `README.md` and the `docs/` site for anything
      describing `AddCache` registration semantics or the package's dependencies. Land
      as a separate `docs:` commit if anything changes.

- [~] 8. **User testing.** Push the branch; user tests from origin. Do NOT open the PR
      until the user confirms the feature is done.

- [ ] 9. **Close the records** (in this PR): backlog file, central requests file,
      and both GitHub issues with evidence.

- [ ] 10. **Close out.** Archive `plan/feature.md` to the Plan directory `done/`,
      `git rm -r plan`, final `fix:` commit, push, open PR.

## Notes

_(updated as work proceeds)_

- **Steps 1, 1b, 2 done** (commit `264e59e`). `Tharga.Blazor` → 2.3.1. All four test
  projects moved to `xunit.v3` 4.0.0 under Microsoft.Testing.Platform:
  - `global.json` selects the MTP runner (`test.runner`).
  - Dropped `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`, `coverlet.collector`
    and `coverlet.msbuild` — all VSTest-only under this model. Added
    `Microsoft.Testing.Extensions.CodeCoverage` for cobertura output.
  - Test projects now need `<OutputType>Exe</OutputType>`; xunit v3 4.x refuses to
    build a library test project.
  - CI test step rewritten: `--filter "(Category!=Integration)&(Category!=TimeCritical)"`
    → `--filter-not-trait "Category=Integration" --filter-not-trait "Category=TimeCritical"`;
    `--collect:"XPlat Code Coverage"` → `--coverage --coverage-output-format cobertura`.
    Verified locally: four cobertura files land in `./coverage` for Codecov.
  - `coverage/` added to `.gitignore`.
- **Baseline (step 2):** unfiltered run = 486 tests, 483 passed, 1 failed — the failure
  is the known-flaky `FetchDataThrottleTests` from backlog item 9
  ("Expected monitorGetEventCount to be 10, but found 9"), i.e. the documented master
  behaviour, not a migration regression. CI-filtered run = 464 tests, **all pass**,
  because the filter excludes the TimeCritical throttle tests.

- **Step 3 done** (commit `8f9b292`). Verified by packing the core project and reading
  the nuspec out of the `.nupkg`: the `net10.0` dependency group now lists only
  `Microsoft.Extensions.Hosting.Abstractions`. Confirmed no source file under
  `Tharga.Cache/` referenced FluentAssertions, and all test projects already declare
  their own reference, so nothing else moved.

- **Steps 4 and 5 done** (commit `3456f25`).
  - New `AddCacheConcurrencyTests` (3 tests, 64 concurrent hosts). Against the old code
    all three failed, reproducing the reporter's exact exception —
    `ArgumentException: Destination array is not long enough` at `Dictionary.CopyTo` ←
    `Enumerable.ToArray` — plus an `InvalidOperationException` about concurrent
    mutation, plus the cross-collection leak (host 2's options contained host 1's type).
  - Distinct cache types per host come from nesting a private generic marker in itself,
    so N hosts need no N declared classes.
  - `AppendPreviousRegistrations` now takes the `IServiceCollection` and reads the prior
    `CacheOptions` off the existing `IOptions<CacheOptions>` descriptor's
    `ImplementationInstance`, before the `RemoveAll` that replaces it. Falls through
    harmlessly when the descriptor is absent or registered by other means.
  - `_configuredPersistTypes` and `ResetRegistrations` deleted; `AddCacheIdempotencyTests`
    no longer needs `IDisposable` or the reset hooks and still passes unchanged.

- **Step 6 verification.**
  - CI-gate command (the one the workflow runs) green **3 out of 3**: 467 tests,
    465 passed, 2 skipped.
  - Full unfiltered suite: 489 tests, 3 failed — all three in `FetchDataThrottleTests`,
    all carrying `[Trait("Category", "TimeCritical")]` and therefore excluded from CI.
    Run in isolation that class passes 2 out of 2, confirming these are the documented
    parallel-load flakiness of backlog item 9 rather than a regression. Worth noting:
    the new concurrency tests deliberately saturate the CPU with 64 parallel host
    builds, which makes that pre-existing flakiness easier to hit in a local full-suite
    run. It does not affect CI, but it strengthens the case for backlog item 9.

- **Step 7 done** (commit `d62d4ba`). `docs/articles/getting-started.md` already stated
  that `AddCache` is idempotent; added that the merge is scoped per service collection
  and that concurrent host construction is safe, naming the reporter's
  `WebApplicationFactory` scenario. `README.md` documents registration but not
  idempotency, so it needed no change; there is no CHANGELOG in this repo.
