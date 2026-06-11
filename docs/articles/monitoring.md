# Monitoring

Tharga.Cache exposes its runtime state three ways: programmatically through `ICacheMonitor`, visually through the Blazor dashboard, and over the Model Context Protocol through the MCP provider.

## ICacheMonitor

Inject `ICacheMonitor` to inspect and manage cache state in code:

```csharp
public class CacheHealthCheck(ICacheMonitor monitor)
{
    public void PrintStats()
    {
        foreach (var typeInfo in monitor.GetInfos())
        {
            Console.WriteLine($"{typeInfo.Type.Name}: {typeInfo.Items.Count} items");
        }

        Console.WriteLine($"Fetch queue: {monitor.GetFetchQueueCount()}");
    }

    public void Cleanup()
    {
        monitor.ClearStale(); // Remove expired items
        monitor.ClearAll();   // Remove everything
    }
}
```

`GetInfos()` returns one entry per tracked type, each carrying its `PersistType` and per-item details (key, size, access count, staleness, expiration, and load time).

## Blazor UI

```
dotnet add package Tharga.Cache.Blazor
```

Drop the components into a Blazor page:

```razor
@page "/cache"
@rendermode InteractiveServer

<Tharga.Cache.Blazor.SummaryView />
<Tharga.Cache.Blazor.ListView />
```

- **SummaryView** shows total item count, total size, fetch-queue depth, and a "Clear Cache" button.
- **ListView** shows a grid of all cached types and their items; an info button opens a detail dialog with the item's JSON content (lazily loaded on expand).

Both views subscribe to monitor events for live refresh. The host page needs `<RadzenComponents />`, the `tharga.blazor.js` script, and `AddBlazoredLocalStorage()` registered.

## MCP provider

```
dotnet add package Tharga.Cache.Mcp
```

Register the provider alongside your other MCP providers:

```csharp
builder.Services.AddThargaMcp(b => b.AddCache());
```

```csharp
app.UseThargaMcp(); // exposes the endpoint at /mcp
```

This surfaces cache state to an AI agent over MCP:

- **Resources** — `cache://types`, `cache://items`, `cache://health`, `cache://queue`.
- **Tools** — `cache.clear_stale`, `cache.clear_all`.

The provider runs on System scope; authorization, scopes, and audit flow through Tharga.Mcp as for any other provider.
