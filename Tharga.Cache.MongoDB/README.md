# Tharga.Cache.MongoDB

[![NuGet](https://img.shields.io/nuget/v/Tharga.Cache.MongoDB)](https://www.nuget.org/packages/Tharga.Cache.MongoDB)
![Nuget](https://img.shields.io/nuget/dt/Tharga.Cache.MongoDB)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

MongoDB persistence backend for [Tharga.Cache](https://www.nuget.org/packages/Tharga.Cache). Persist cached data to MongoDB for durability and cross-instance sharing.

## Get Started

```bash
dotnet add package Tharga.Cache
dotnet add package Tharga.Cache.MongoDB
```

```csharp
builder.Services.AddCache(o =>
{
    o.AddMongoDBOptions(m =>
    {
        m.CollectionName = "_cache";
        m.ConfigurationName = "Default";
    });

    o.RegisterType<AuditLog, IMongoDB>();
});
```

Any type registered with `IMongoDB` is persisted to MongoDB. Unregistered types default to in-memory caching.

## Why MongoDB?

- **Durable storage** — cached data survives restarts and deployments
- **Flexible documents** — no rigid schema required for cached types
- **Queryable** — inspect cached data directly in your database when needed

## Documentation

Full documentation, configuration options, and samples are available on the [GitHub project page](https://github.com/Tharga/Cache).

## License

MIT
