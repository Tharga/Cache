# Tharga.Cache

[![NuGet](https://img.shields.io/nuget/v/Tharga.Cache)](https://www.nuget.org/packages/Tharga.Cache)
![Nuget](https://img.shields.io/nuget/dt/Tharga.Cache)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A flexible .NET caching library with multiple cache strategies, configurable eviction policies, and stale-while-revalidate support — all with a clean DI-first API.

## Features

- **Four cache strategies** — Eternal, Time-To-Live (TTL), Time-To-Idle (TTI), and Scoped
- **Eviction policies** — LRU, FIFO, or Random when size or count limits are reached
- **Stale-while-revalidate** — return cached data instantly while refreshing in the background
- **Pluggable persistence** — in-memory by default, with optional Redis, MongoDB, and file backends
- **Composite keys** — build cache keys from multiple parts with `KeyBuilder`
- **Monitoring** — inspect cache state, item counts, and fetch queue depth via `ICacheMonitor`
- **Background cleanup** — built-in WatchDog service removes stale entries automatically

## Get Started

```bash
dotnet add package Tharga.Cache
```

```csharp
// Register
builder.Services.AddCache();

// Use
public class MyService
{
    private readonly ITimeToLiveCache _cache;

    public MyService(ITimeToLiveCache cache) => _cache = cache;

    public Task<Product> GetProductAsync(int id) =>
        _cache.GetAsync<Product>($"product-{id}", () => LoadAsync(id), TimeSpan.FromMinutes(10));
}
```

## Persistence Backends

Extend with optional packages to persist beyond memory:

| Package | Backend |
|---------|---------|
| [Tharga.Cache.Redis](https://www.nuget.org/packages/Tharga.Cache.Redis) | Redis |
| [Tharga.Cache.MongoDB](https://www.nuget.org/packages/Tharga.Cache.MongoDB) | MongoDB |
| [Tharga.Cache.File](https://www.nuget.org/packages/Tharga.Cache.File) | Local files |

## Documentation

Full documentation, configuration options, and samples are available on the [GitHub project page](https://github.com/Tharga/Cache).

## License

MIT
