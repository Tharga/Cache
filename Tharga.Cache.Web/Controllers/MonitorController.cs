using Microsoft.AspNetCore.Mvc;

namespace Tharga.Cache.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class MonitorController : ControllerBase
{
    private readonly ICacheMonitor _cacheMonitor;

    public MonitorController(ICacheMonitor cacheMonitor)
    {
        _cacheMonitor = cacheMonitor;
    }

    [HttpGet]
    public async Task<IActionResult> Keys()
    {
        var typeInfos = _cacheMonitor.GetInfos().ToArray();
        var keys = typeInfos.SelectMany(x => x.Items.Keys).ToArray();
        var infos = typeInfos.SelectMany(x => x.Items).Select(x => new CacheInfo
        {
            Key = x.Key,
            AccessCount = x.Value.AccessCount
        }).ToArray();
        return Ok(new MonitorData
        {
            //Keys = keys,
            Infos = infos
        });
    }
}