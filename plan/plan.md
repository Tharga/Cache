# Plan

## Steps

- [~] 1. **NuGet updates up front (whole solution).**
      Available: `Tharga.Blazor` 2.3.0 → 2.3.1 (patch); `xunit.v3` 3.2.2 → 4.0.0 and
      `xunit.runner.visualstudio` 3.1.5 → 4.0.0 (major) across the four test projects.
      Apply, then verify build + full suite before any fix code is written.
      Note: `dotnet outdated` is not installed on this machine; used
      `dotnet list package --outdated` instead.

- [ ] 1b. **Microsoft.Testing.Platform migration** (forced by the xunit v4 major).
      xunit.v3 4.0.0 drops the VSTest bridge on the .NET 10 SDK — `dotnet test` fails
      with *"Testing with VSTest target is no longer supported"* until the project opts
      into MTP. User chose (2026-08-15) to do the full migration in this PR rather than
      defer it. Requires: `global.json` runner opt-in, dropping the VSTest-only
      packages, translating the CI `--filter` trait expression to xunit's MTP filter
      options, and replacing `--collect:"XPlat Code Coverage"` with an MTP coverage
      route that still emits cobertura into `./coverage` for Codecov.

- [ ] 2. **Baseline the suite on the branch.** Backlog item 9 records
      `FetchDataThrottleTests` as failing most runs on unmodified master. Establish
      how it behaves here so a later failure is not misread as caused by the fixes.

- [ ] 3. **gh#56 — remove the FluentAssertions reference** from
      `Tharga.Cache/Tharga.Cache.csproj`. Verify the packed nuspec no longer lists it.

- [ ] 4. **gh#55 — write the failing test first.** Concurrent `AddCache` across many
      independent `ServiceCollection`s: assert no throw, and assert no registration
      bleeds from one collection into another.

- [ ] 5. **gh#55 — implement.** Replace the static dictionary with a merge that reads
      the previous `CacheOptions` off the existing `IOptions<CacheOptions>` descriptor.
      Delete `ResetRegistrations` and the test hooks that call it.

- [ ] 6. **Full verification.** Release build + full suite, repeated runs to separate
      real regressions from the known throttle-test flakiness.

- [ ] 7. **Documentation review.** Check `README.md` and the `docs/` site for anything
      describing `AddCache` registration semantics or the package's dependencies. Land
      as a separate `docs:` commit if anything changes.

- [ ] 8. **Close the records** (in this PR): backlog file, central requests file,
      and both GitHub issues with evidence.

- [ ] 9. **Close out.** Archive `plan/feature.md` to the Plan directory `done/`,
      `git rm -r plan`, final `fix:` commit, push, open PR.

## Notes

_(updated as work proceeds)_

- **Step 1 in progress** — sweep run, updates identified, not yet applied.
