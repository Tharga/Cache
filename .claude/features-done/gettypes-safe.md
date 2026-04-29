# Feature: gettypes-safe

## Goal
Make `AddCache()` resilient to `ReflectionTypeLoadException` thrown by `Assembly.GetTypes()` when an unrelated assembly has unresolvable metadata references. PlutusWave hit a fatal startup crash from a Quilt4Net/ApplicationInsights mismatch in a third-party dll.

## Scope
- `Tharga.Cache/CacheRegistrationExtensions.cs` — add `GetTypesSafe(assembly, logger)` helper, replace `assembly.GetTypes()` calls in `RegisterIPersistFromAssembly` and `InvokeAllPersistRegistrations`.
- Add a test verifying `AddCache` does not throw when a loaded assembly has an unresolvable type.

## Acceptance Criteria
- `AddCache` completes when an in-process assembly throws `ReflectionTypeLoadException` from `GetTypes()`.
- A warning is logged (per affected assembly) so the root cause isn't silently swallowed.
- Existing tests still pass.

## Done Condition
User confirms the fix is satisfactory.

## Originating Branch
develop
