# Tharga.Cache.File

[![NuGet](https://img.shields.io/nuget/v/Tharga.Cache.File)](https://www.nuget.org/packages/Tharga.Cache.File)
![Nuget](https://img.shields.io/nuget/dt/Tharga.Cache.File)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

File-based persistence backend for [Tharga.Cache](https://www.nuget.org/packages/Tharga.Cache). Persist cached data to the local filesystem with multiple serialization formats.

## Get Started

```bash
dotnet add package Tharga.Cache
dotnet add package Tharga.Cache.File
```

```csharp
builder.Services.AddCache(o =>
{
    o.AddFileDBOptions(f =>
    {
        f.CompanyName = "MyCompany";
        f.AppName = "MyApp";
        f.Format = Format.Json;
    });

    o.RegisterType<Settings, IFile>();
});
```

## Serialization Formats

| Format | Description |
|--------|-------------|
| `Json` | Human-readable JSON (default) |
| `Base64` | Base64-encoded |
| `GZip` | GZip-compressed for smaller files |
| `Brotli` | Brotli-compressed for maximum compression |

## Why File Persistence?

- **Zero infrastructure** — no database or server needed
- **Survives restarts** — cached data persists on disk
- **Great for desktop and console apps** — local storage without external dependencies
- **Compression options** — reduce disk usage with GZip or Brotli

## Documentation

Full documentation, configuration options, and samples are available on the [GitHub project page](https://github.com/Tharga/Cache).

## License

MIT
