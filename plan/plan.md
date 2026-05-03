# Plan: cache-mcp

## Steps
- [x] 1. Create `Tharga.Cache.Mcp/` project skeleton (csproj + README) and add to solution — net8/9/10, Tharga.Mcp 0.1.3, ProjectReference to Tharga.Cache, builds cleanly
- [ ] 2. Implement `ThargaMcpBuilderExtensions.AddCache()` extension
- [ ] 3. Implement `CacheResourceProvider` with 4 resources (`cache://types`, `cache://items`, `cache://health`, `cache://queue`)
- [ ] 4. Implement `CacheToolProvider` with 2 tools (`cache.clear_stale`, `cache.clear_all`)
- [ ] 5. Add unit tests (resource list/read, tool list/invoke against mock `ICacheMonitor`)
- [ ] 6. Add `Tharga.Cache.Mcp.csproj` to both Pack steps in `.github/workflows/build.yml`
- [ ] 7. Build, run full test suite, commit
