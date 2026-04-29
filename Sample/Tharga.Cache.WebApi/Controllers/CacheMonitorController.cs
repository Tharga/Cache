using Microsoft.AspNetCore.Mvc;

namespace Tharga.Cache.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class CacheMonitorController : ControllerBase
{
    private readonly ICacheMonitor _cacheMonitor;

    public CacheMonitorController(ICacheMonitor cacheMonitor)
    {
        _cacheMonitor = cacheMonitor;
    }

    [HttpGet]
    public Task<IActionResult> GetCache()
    {
        var infos = _cacheMonitor.GetInfos();
        var response = infos.Select(x => new
        {
            Type = x.Type.Name,
            ItemCount = x.Items.Count,
            Size = x.Items.Sum(y => y.Value.Size),
            TotalAccessCount = x.Items.Sum(y => y.Value.AccessCount)
        });
        return Task.FromResult<IActionResult>(Ok(response));
    }

    [HttpGet("type/{type}")]
    public Task<IActionResult> GetType(string type)
    {
        var info = _cacheMonitor.GetInfos().FirstOrDefault(x => x.Type.Name == type);
        if (info == null) return Task.FromResult<IActionResult>(NotFound($"Cannot find cache type '{type}'."));

        var response = info.Items.Select(x => new
        {
            x.Key,
            x.Value.AccessCount,
            x.Value.Size,
            x.Value.CreateTime,
            x.Value.LastAccessTime,
        });
        return Task.FromResult<IActionResult>(Ok(response));
    }

    [HttpGet("fetchQueue")]
    public Task<IActionResult> GetQueueCount()
    {
        return Task.FromResult<IActionResult>(Ok(_cacheMonitor.GetFetchQueueCount()));
    }
}