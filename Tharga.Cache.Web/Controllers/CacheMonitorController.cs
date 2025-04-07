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
    public async Task<IActionResult> GetCache()
    {
        var infos = _cacheMonitor.GetInfos();
        var response = infos.Select(x => new { Type = x.Type.Name, ItemCount = x.Items.Count, Size = x.Items.Sum(y => y.Value.Size) });
        return Ok(response);
    }
}