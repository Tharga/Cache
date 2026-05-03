# Feature: cache-mcp

## Goal
Add a new package `Tharga.Cache.Mcp` that exposes cache state via MCP, following the established `Tharga.Platform.Mcp` / `Tharga.MongoDB.Mcp` / `Tharga.Communication.Mcp` pattern. Consumers register with `builder.Services.AddThargaMcp(b => b.AddCache())`.

## Originating request
`Tharga.Cache — MCP / MCP Provider for Cache monitoring` from "All products using Cache" (Quilt4Net Server, PlutusWave, Florida, Eplicta), priority Medium.

## Decisions (open questions resolved)

| Question | Decision |
|---|---|
| Method name | `AddCache()` on `IThargaMcpBuilder` — matches Platform/MongoDB/Communication pattern. The original request said `AddMcpCache()`, but established convention wins. |
| Per-key evict (`cache.evict`) | **Skip in v1.** A clean implementation would need a new method on `ICacheMonitor` (e.g. `EvictAsync(Type, Key)`); reflection via `IEternalCache.DropAsync<T>(key)` is hacky. Add as a follow-up if a consumer asks. |
| Hit/miss counters | **Skip.** Today `CacheItemInfo` only tracks `AccessCount` (total reads). True hit/miss requires adding miss tracking to `CacheBase`/`CacheMonitor` — out of scope for the MCP package. Track as a follow-up if needed. |

## Scope

### New project: `Tharga.Cache.Mcp/`
- `Tharga.Cache.Mcp.csproj` — `net8.0;net9.0;net10.0`, package metadata matching the other Tharga.Cache packages, dependency on `Tharga.Mcp` v0.1.3, project reference to `Tharga.Cache`.
- `README.md` — concise NuGet sales pitch.
- `ThargaMcpBuilderExtensions.cs` — `AddCache()` extension on `IThargaMcpBuilder`.
- `CacheResourceProvider.cs` — read-only data, scope `McpScope.System`.
- `CacheToolProvider.cs` — actions, scope `McpScope.System`.

### Resources

| URI | Source | Payload |
|---|---|---|
| `cache://types` | `ICacheMonitor.GetInfos()` | per type: type name, persist type name (Memory/Redis/MongoDB/File), item count, total size, `StaleWhileRevalidate`, `ReturnDefaultOnFirstLoad` |
| `cache://items` | `ICacheMonitor.GetInfos()` flattened | per item: type, key, size, fresh span, expires, last accessed, access count, load duration, is stale |
| `cache://health` | `ICacheMonitor.GetHealthTypes()` | per persist type: name, success, message |
| `cache://queue` | `ICacheMonitor.GetFetchQueueCount()` | { queueDepth } |

### Tools

| Name | Action |
|---|---|
| `cache.clear_stale` | `ICacheMonitor.ClearStale()` |
| `cache.clear_all` | `ICacheMonitor.ClearAll()` |

### Tests
Basic unit tests in a new `Tharga.Cache.Mcp.Tests` project (or possibly added to `Tharga.Cache.Tests`):
- Resource provider lists all 4 resource descriptors.
- Each resource read returns a non-empty payload against a populated mock `ICacheMonitor`.
- Tool provider lists both tools.
- Each tool invocation calls the corresponding monitor method.

### Solution + CI
- Add `Tharga.Cache.Mcp.csproj` to `Tharga.Cache.sln`.
- Add a pack line to both stable and pre-release Pack steps in `.github/workflows/build.yml`.

## Acceptance criteria
- `builder.Services.AddThargaMcp(b => b.AddCache())` registers without error.
- All four resources are discoverable and readable through the MCP endpoint.
- Both tools execute and report success.
- Existing tests still pass; the new test project passes.
- Package `Tharga.Cache.Mcp` is built by CI (stable on master, prerelease on PRs).

## Done condition
User confirms the package works against an MCP client (or at minimum, the integration test passes).

## Originating branch
develop
