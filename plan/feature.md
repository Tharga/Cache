# Fix: registration race (gh#55) and FluentAssertions dependency leak (gh#56)

## Goal

Close two consumer-reported defects in the published `Tharga.Cache` packages:

1. **gh#55** — `AddCache` is not safe to call concurrently. A process-wide `static`
   dictionary is read-and-merged without synchronization, so two hosts built at the
   same time in one process can throw
   `ArgumentException: Destination array is not long enough`.
2. **gh#56** — `Tharga.Cache` ships **FluentAssertions** as a public NuGet dependency,
   propagating the Xceed Community License obligation to every consumer and causing
   `NU1605` downgrade errors for consumers pinning an older version.

## Scope

- `Tharga.Cache/CacheRegistrationExtensions.cs` — remove the process-wide static
  registration state; merge previous registrations from the `IServiceCollection`
  instead.
- `Tharga.Cache/Tharga.Cache.csproj` — remove the unused FluentAssertions reference.
- Tests covering both.
- NuGet package updates across the whole solution, up front (per Feature Workflow).

Out of scope: backlog item 9 (`FetchDataThrottleTests` flakiness). It is a known
pre-existing failure on unmodified master and is not caused by, nor fixed by, this
work — but it does mean the suite needs repeat runs to be trusted.

## Approach

### gh#55 — scope the merge state to the container

The static dictionary is not merely unsynchronized, it is **process-wide state behind
a per-container API**: one host's type registrations leak into an unrelated host's
merged options. The tests already work around this by calling an `internal`
`ResetRegistrations()` from their constructor and `Dispose`.

`AddCache` already replaces `IOptions<CacheOptions>` on every call, so the previous
call's `CacheOptions` is available as the existing descriptor's `ImplementationInstance`
before it is removed. Merging from there removes the shared state entirely — no lock,
no cross-host leak, and `ResetRegistrations` and its test hooks can go.

Conflict semantics are preserved for the case that matters: with two calls registering
the same type, the later call still wins in the merged options, as today.

### gh#56 — remove the reference

No source file under `Tharga.Cache/` references FluentAssertions. All five test
projects already declare their own reference, so removal costs nothing. The dependency
flows into all six published packages because the siblings `ProjectReference` the core.

## Acceptance criteria

- [ ] Building N `ServiceCollection`s concurrently through `AddCache` neither throws
      nor bleeds registrations between them, proven by a test.
- [ ] No `static` mutable registration state remains in `CacheRegistrationExtensions`.
- [ ] `ResetRegistrations` is gone, and `AddCacheIdempotencyTests` passes without it.
- [ ] `Tharga.Cache.csproj` declares no FluentAssertions reference; the packed nuspec
      lists only `Microsoft.Extensions.Hosting.Abstractions`.
- [ ] Existing `AddCache` idempotency behaviour is unchanged.
- [ ] Solution builds Release and the full suite passes (allowing for the known
      `FetchDataThrottleTests` flakiness, which must be no worse than the master baseline).

## Done condition

Both issues closed with evidence, records swept (backlog, central requests file,
GitHub issues), and the PR merged to master.
