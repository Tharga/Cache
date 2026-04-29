# Plan: gettypes-safe

## Steps
- [~] 1. Add `GetTypesSafe` helper in `CacheRegistrationExtensions.cs`
- [ ] 2. Replace both `assembly.GetTypes()` call sites
- [ ] 3. Pass an `ILogger` (best-effort) so the warning surfaces
- [ ] 4. Write a regression test that triggers `ReflectionTypeLoadException`
- [ ] 5. Build, run full test suite, commit
