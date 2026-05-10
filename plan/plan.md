# Plan: cache-content-darkmode

## Steps
- [x] 1. Update `CacheContent.razor` — replaced `background:#f5f5f5` with `var(--rz-base)`, added `color: var(--rz-text-color)`.
- [x] 2. Bundle dependency bumps (NuGet update reminder during feature start):
      - Tharga.Blazor 2.1.4 → 2.1.5 in `Tharga.Cache.Blazor.csproj`.
      - Tharga.MongoDB 2.10.9 → 2.10.10 in `Tharga.Cache.MongoDB.csproj` (clears NU1902/NU1903 advisories from transitive SharpCompress/Snappier).
- [x] 3. Build clean (1 pre-existing CS0162 warning), 453 tests pass.
- [ ] 4. Commit.
- [ ] 5. (After user verifies dark mode in browser) close feature, push, create PR.
