# Tharga.Cache.Blazor

[![NuGet](https://img.shields.io/nuget/v/Tharga.Cache.Blazor)](https://www.nuget.org/packages/Tharga.Cache.Blazor)
![Nuget](https://img.shields.io/nuget/dt/Tharga.Cache.Blazor)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Blazor monitoring UI components for [Tharga.Cache](https://www.nuget.org/packages/Tharga.Cache). Add a visual cache dashboard to your Blazor application with two lines of markup.

## Get Started

```bash
dotnet add package Tharga.Cache
dotnet add package Tharga.Cache.Blazor
```

Add the components to any Blazor page:

```razor
@page "/cache"
@rendermode InteractiveServer

<Tharga.Cache.Blazor.SummaryView />
<Tharga.Cache.Blazor.ListView />
```

## Components

### SummaryView

Displays total item count, total cache size, fetch queue depth, and a "Clear Cache" button.

### ListView

A hierarchical data grid showing all cached types and their items. For each item, view the key, creation time, expiration, last access, size, access count, and staleness.

## Why Add a Cache Dashboard?

- **Visibility** — see what is cached and how much memory it uses
- **Debugging** — inspect individual cache entries and their expiration state
- **Operations** — clear stale or all cached data with a single click

## Documentation

Full documentation, configuration options, and samples are available on the [GitHub project page](https://github.com/Tharga/Cache).

## License

MIT
