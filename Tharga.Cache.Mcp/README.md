# Tharga.Cache.Mcp

[![NuGet](https://img.shields.io/nuget/v/Tharga.Cache.Mcp)](https://www.nuget.org/packages/Tharga.Cache.Mcp)
![Nuget](https://img.shields.io/nuget/dt/Tharga.Cache.Mcp)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

MCP (Model Context Protocol) provider for [Tharga.Cache](https://www.nuget.org/packages/Tharga.Cache). Lets an MCP-aware AI client browse cached data and run cache maintenance actions. Plugs into [Tharga.Mcp](https://www.nuget.org/packages/Tharga.Mcp).

## Get Started

```bash
dotnet add package Tharga.Cache
dotnet add package Tharga.Cache.Mcp
```

```csharp
builder.Services.AddCache();
builder.Services.AddThargaMcp(b => b.AddCache());

// ...
app.UseThargaMcp();
```

## Resources

| URI | Description |
|-----|-------------|
| `cache://types` | All registered cache types with persistence backend, item count, total size, and config flags |
| `cache://items` | Flat list of cached items with key, size, fresh span, expires, last accessed, access count, load duration, stale |
| `cache://health` | Health status of each persistence backend (Memory, Redis, MongoDB, File) |
| `cache://queue` | Current fetch queue depth |

## Tools

| Name | Action |
|------|--------|
| `cache.clear_stale` | Evicts all stale items |
| `cache.clear_all` | Evicts everything from all caches |

## Why expose the cache via MCP?

- **Visibility** — let an AI assistant inspect what's cached and how much memory it uses without granting it backend access
- **Operations** — clear stale or all cached data through the same channel
- **Diagnostics** — backend health is one resource read away

## Documentation

Full documentation, configuration options, and samples are available on the [GitHub project page](https://github.com/Tharga/Cache).

## License

MIT
