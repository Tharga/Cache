using Microsoft.AspNetCore.Mvc;

namespace Tharga.Cache.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class MemoryCacheController : ControllerBase
{
    private readonly ITimeToLiveCache _timeToLiveCache;

    public MemoryCacheController(ITimeToLiveCache timeToLiveCache)
    {
        _timeToLiveCache = timeToLiveCache;
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> Get(string key)
    {
        var item = await _timeToLiveCache.GetAsync(key, () => Task.FromResult(new MemoryData { Guid = Guid.NewGuid() }));
        return Ok(item);
    }

    [HttpDelete("{key}")]
    public async Task<IActionResult> Delete(string key)
    {
        var item = await _timeToLiveCache.DropAsync<MemoryData>(key);
        return Ok(item);
    }
}