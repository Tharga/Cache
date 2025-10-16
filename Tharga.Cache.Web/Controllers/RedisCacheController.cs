using Microsoft.AspNetCore.Mvc;

namespace Tharga.Cache.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class RedisCacheController : ControllerBase
{
    private readonly ITimeToLiveCache _timeToLiveCache;

    public RedisCacheController(ITimeToLiveCache timeToLiveCache)
    {
        _timeToLiveCache = timeToLiveCache;
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> Get(string key)
    {
        var item = await _timeToLiveCache.GetAsync(key, () => Task.FromResult(new RedisData { Guid = Guid.NewGuid() }));
        return Ok(item);
    }
}