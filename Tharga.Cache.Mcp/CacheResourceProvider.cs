using System.Text.Json;
using Tharga.Mcp;

namespace Tharga.Cache.Mcp;

/// <summary>
/// Exposes Tharga.Cache monitoring data as MCP resources on the System scope.
/// </summary>
public sealed class CacheResourceProvider : IMcpResourceProvider
{
    internal const string TypesUri = "cache://types";
    internal const string ItemsUri = "cache://items";
    internal const string HealthUri = "cache://health";
    internal const string QueueUri = "cache://queue";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    private readonly ICacheMonitor _monitor;

    public CacheResourceProvider(ICacheMonitor monitor)
    {
        _monitor = monitor;
    }

    public McpScope Scope => McpScope.System;

    public Task<IReadOnlyList<McpResourceDescriptor>> ListResourcesAsync(IMcpContext context, CancellationToken cancellationToken)
    {
        IReadOnlyList<McpResourceDescriptor> resources =
        [
            new McpResourceDescriptor
            {
                Uri = TypesUri,
                Name = "Cache Types",
                Description = "Registered cache types with persistence backend (Memory/Redis/MongoDB/File), item count, total size, and config flags.",
                MimeType = "application/json",
            },
            new McpResourceDescriptor
            {
                Uri = ItemsUri,
                Name = "Cache Items",
                Description = "Flat list of cached items: type, key, size, fresh span, expires, last accessed, access count, load duration, stale.",
                MimeType = "application/json",
            },
            new McpResourceDescriptor
            {
                Uri = HealthUri,
                Name = "Cache Persistence Health",
                Description = "Connectivity status of each persistence backend.",
                MimeType = "application/json",
            },
            new McpResourceDescriptor
            {
                Uri = QueueUri,
                Name = "Cache Fetch Queue",
                Description = "Current depth of the in-flight fetch queue.",
                MimeType = "application/json",
            },
        ];
        return Task.FromResult(resources);
    }

    public async Task<McpResourceContent> ReadResourceAsync(string uri, IMcpContext context, CancellationToken cancellationToken)
    {
        return uri switch
        {
            TypesUri => BuildTypes(),
            ItemsUri => BuildItems(),
            HealthUri => await BuildHealthAsync(),
            QueueUri => BuildQueue(),
            _ => new McpResourceContent { Uri = uri, Text = $"Unknown resource: {uri}" },
        };
    }

    private McpResourceContent BuildTypes()
    {
        var types = _monitor.GetInfos().Select(info => new
        {
            type = info.Type.FullName,
            persistType = FormatPersistType(info.PersistType),
            count = info.Items.Count,
            totalSize = info.Items.Sum(x => x.Value.Size),
            staleWhileRevalidate = info.StaleWhileRevalidate,
            returnDefaultOnFirstLoad = info.ReturnDefaultOnFirstLoad,
        }).ToArray();

        return new McpResourceContent
        {
            Uri = TypesUri,
            MimeType = "application/json",
            Text = JsonSerializer.Serialize(new { types }, JsonOptions),
        };
    }

    private McpResourceContent BuildItems()
    {
        var items = _monitor.GetInfos().SelectMany(info => info.Items.Select(item => new
        {
            type = info.Type.FullName,
            persistType = FormatPersistType(info.PersistType),
            key = item.Key,
            size = item.Value.Size,
            freshSpan = item.Value.FreshSpan,
            createTime = item.Value.CreateTime,
            updateTime = item.Value.UpdateTime,
            expireTime = item.Value.ExpireTime,
            lastAccessTime = item.Value.LastAccessTime,
            accessCount = item.Value.AccessCount,
            loadDuration = item.Value.LoadDuration,
            isStale = item.Value.IsStale,
        })).ToArray();

        return new McpResourceContent
        {
            Uri = ItemsUri,
            MimeType = "application/json",
            Text = JsonSerializer.Serialize(new { items }, JsonOptions),
        };
    }

    private async Task<McpResourceContent> BuildHealthAsync()
    {
        var results = new List<object>();
        foreach (var healthType in _monitor.GetHealthTypes())
        {
            var health = await healthType.GetHealthAsync();
            results.Add(new
            {
                type = healthType.Type,
                success = health.Success,
                message = health.Message,
            });
        }

        return new McpResourceContent
        {
            Uri = HealthUri,
            MimeType = "application/json",
            Text = JsonSerializer.Serialize(new { health = results }, JsonOptions),
        };
    }

    private McpResourceContent BuildQueue()
    {
        var payload = new { queueDepth = _monitor.GetFetchQueueCount() };
        return new McpResourceContent
        {
            Uri = QueueUri,
            MimeType = "application/json",
            Text = JsonSerializer.Serialize(payload, JsonOptions),
        };
    }

    private static string FormatPersistType(Type persistType)
    {
        if (persistType == null) return null;
        var name = persistType.Name;
        return name.StartsWith("I") && name.Length > 1 && char.IsUpper(name[1]) ? name.Substring(1) : name;
    }
}
