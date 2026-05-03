using System.Text.Json;
using Tharga.Mcp;

namespace Tharga.Cache.Mcp;

/// <summary>
/// Exposes Tharga.Cache maintenance actions as MCP tools on the System scope.
/// </summary>
public sealed class CacheToolProvider : IMcpToolProvider
{
    internal const string ClearStaleToolName = "cache.clear_stale";
    internal const string ClearAllToolName = "cache.clear_all";

    private static readonly JsonElement EmptyArgsSchema = JsonSerializer.Deserialize<JsonElement>("""
        {
          "type": "object",
          "properties": {}
        }
        """);

    private readonly ICacheMonitor _monitor;

    public CacheToolProvider(ICacheMonitor monitor)
    {
        _monitor = monitor;
    }

    public McpScope Scope => McpScope.System;

    public Task<IReadOnlyList<McpToolDescriptor>> ListToolsAsync(IMcpContext context, CancellationToken cancellationToken)
    {
        IReadOnlyList<McpToolDescriptor> tools =
        [
            new McpToolDescriptor
            {
                Name = ClearStaleToolName,
                Description = "Evict all stale cache items across all registered cache types.",
                InputSchema = EmptyArgsSchema,
            },
            new McpToolDescriptor
            {
                Name = ClearAllToolName,
                Description = "Evict every item from every registered cache type.",
                InputSchema = EmptyArgsSchema,
            },
        ];
        return Task.FromResult(tools);
    }

    public Task<McpToolResult> CallToolAsync(string toolName, JsonElement arguments, IMcpContext context, CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(toolName switch
            {
                ClearStaleToolName => ClearStale(),
                ClearAllToolName => ClearAll(),
                _ => Error($"Unknown tool: {toolName}"),
            });
        }
        catch (Exception e)
        {
            return Task.FromResult(Error(e.Message));
        }
    }

    private McpToolResult ClearStale()
    {
        _monitor.ClearStale();
        return Ok(new { cleared = "stale" });
    }

    private McpToolResult ClearAll()
    {
        _monitor.ClearAll();
        return Ok(new { cleared = "all" });
    }

    private static McpToolResult Ok(object payload)
    {
        return new McpToolResult
        {
            Content = [new McpContent { Type = "text", Text = JsonSerializer.Serialize(payload) }],
        };
    }

    private static McpToolResult Error(string message)
    {
        return new McpToolResult
        {
            IsError = true,
            Content = [new McpContent { Type = "text", Text = message }],
        };
    }
}
