# Plan: cache-mcp

## Steps
- [x] 1. Create `Tharga.Cache.Mcp/` project skeleton (csproj + README) and add to solution — net8/9/10, Tharga.Mcp 0.1.3, ProjectReference to Tharga.Cache, builds cleanly
- [x] 2. Implement `ThargaMcpBuilderExtensions.AddCache()` extension
- [x] 3. Implement `CacheResourceProvider` with 4 resources (`cache://types`, `cache://items`, `cache://health`, `cache://queue`)
- [x] 4. Implement `CacheToolProvider` with 2 tools (`cache.clear_stale`, `cache.clear_all`)
- [x] 5. Add unit tests (resource list/read, tool list/invoke against mock `ICacheMonitor`) — 10 tests, all green
- [ ] 6. Wire MCP into the `Tharga.Cache.WebApi` sample (`AddThargaMcp(b => b.AddCache())` + `app.UseThargaMcp()`)
- [ ] 7. Add `Tharga.Cache.Mcp.csproj` to both Pack steps in `.github/workflows/build.yml`
- [ ] 8. Build, run full test suite, commit
