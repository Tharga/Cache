using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Tharga.Cache.Web;

namespace Tharga.Cache.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class MoreWeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    private readonly ITimeToLiveCache _ttlCache;

    public MoreWeatherForecastController(ITimeToLiveCache ttlCache)
    {
        _ttlCache = ttlCache;
    }

    [HttpGet("cache_same")]
    public async Task<IActionResult> GetSame()
    {
        return await GetDataAsync("Same");
    }

    [HttpGet("cache_new")]
    public async Task<IActionResult> GetNew()
    {
        return await GetDataAsync(null);
    }

    [HttpGet("many_slow")]
    public async Task<IActionResult> ManySlow()
    {
        return await GetDataAsync(null, 10, TimeSpan.FromSeconds(2));
    }

    private async Task<IActionResult> GetDataAsync(string cacheKey, int count = 3, TimeSpan? delayPerItem = null)
    {
        var sw = Stopwatch.StartNew();

        var response = Enumerable.Range(0, count).Select(async index =>
            {
                var item = await _ttlCache.GetAsync(cacheKey ?? Guid.NewGuid().ToString(), async () =>
                {
                    await Task.Delay(delayPerItem ?? TimeSpan.Zero);
                    return new WeatherForecast
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        TemperatureC = Random.Shared.Next(-20, 55),
                        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                    };
                }, TimeSpan.FromSeconds(10));
                return item;
            })
            .ToArray();

        sw.Stop();

        Response.Headers.TryAdd("LoadTime", $"{sw.Elapsed.TotalMilliseconds}ms");

        return Ok(response);
    }
}