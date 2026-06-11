---
_layout: landing
---

# Tharga.Cache

A flexible .NET caching library with multiple cache strategies, pluggable persistence backends, eviction policies, and a Blazor monitoring UI. Register one line in DI, inject a cache, and get-or-load with a fetch delegate — the first call loads, subsequent calls within the fresh span return the cached value.

## Packages

| Package | What it does |
|---|---|
| [Tharga.Cache](https://www.nuget.org/packages/Tharga.Cache) | Core library with in-memory caching, the four cache interfaces, options, key building, and monitoring. |
| [Tharga.Cache.Redis](https://www.nuget.org/packages/Tharga.Cache.Redis) | Redis persistence backend (`IRedis`). |
| [Tharga.Cache.MongoDB](https://www.nuget.org/packages/Tharga.Cache.MongoDB) | MongoDB persistence backend (`IMongoDB`). |
| [Tharga.Cache.File](https://www.nuget.org/packages/Tharga.Cache.File) | File-based persistence backend (`IFile`). |
| [Tharga.Cache.Blazor](https://www.nuget.org/packages/Tharga.Cache.Blazor) | Blazor monitoring UI components. |
| [Tharga.Cache.Mcp](https://www.nuget.org/packages/Tharga.Cache.Mcp) | MCP (Model Context Protocol) provider for cache monitoring. |

## Quick start

```
dotnet add package Tharga.Cache
```

```csharp
builder.Services.AddCache();
```

```csharp
public class WeatherService(ITimeToLiveCache cache)
{
    public Task<WeatherForecast[]> GetForecastAsync() =>
        cache.GetAsync<WeatherForecast[]>(
            "weather-forecast",
            () => LoadFromApiAsync(),
            TimeSpan.FromMinutes(5));
}
```

See [Getting started](articles/getting-started.md) for the full walkthrough.

## What's in the box

- **Four cache strategies** — `IEternalCache` (never expires), `ITimeToLiveCache` (TTL), `ITimeToIdleCache` (TTI, clock resets on access), and `IScopeCache` (per-DI-scope). See [Cache types](articles/cache-types.md).
- **Pluggable backends** — in-memory by default, with Redis, MongoDB, and file backends you can assign per type and freely mix. See [Persistence backends](articles/persistence-backends.md).
- **Smart loading** — stale-while-revalidate, background refresh, eviction policies (FIFO / LRU / random), and size/count limits. See [Configuration](articles/getting-started.md#configuration).
- **Monitoring** — inspect cache state via `ICacheMonitor`, a Blazor dashboard, or over MCP. See [Monitoring](articles/monitoring.md).

## Repo

[github.com/Tharga/Cache](https://github.com/Tharga/Cache) — source, issues, releases.
