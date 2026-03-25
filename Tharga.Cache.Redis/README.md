# Tharga.Cache.Redis

[![NuGet](https://img.shields.io/nuget/v/Tharga.Cache.Redis)](https://www.nuget.org/packages/Tharga.Cache.Redis)
![Nuget](https://img.shields.io/nuget/dt/Tharga.Cache.Redis)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Redis persistence backend for [Tharga.Cache](https://www.nuget.org/packages/Tharga.Cache). Store cached data in Redis for sharing across instances or surviving application restarts.

## Get Started

```bash
dotnet add package Tharga.Cache
dotnet add package Tharga.Cache.Redis
```

```csharp
builder.Services.AddCache(o =>
{
    o.AddRedisDBOptions(r =>
        r.ConnectionStringLoader = sp => "localhost:6379");

    o.RegisterType<SessionData, IRedis>();
});
```

Any type registered with `IRedis` is persisted to Redis. Unregistered types default to in-memory caching.

## Why Redis?

- **Shared cache** across multiple application instances
- **Survives restarts** — cached data is not lost on deploy
- **High throughput** with low latency

## Documentation

Full documentation, configuration options, and samples are available on the [GitHub project page](https://github.com/Tharga/Cache).

## License

MIT
