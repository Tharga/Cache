using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Tharga.Cache.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    private readonly ITimeToLiveCache _ttlCache;
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ITimeToLiveCache ttlCache, ILogger<WeatherForecastController> logger)
    {
        _ttlCache = ttlCache;
        _logger = logger;
    }

    //[HttpGet("Get")]
    //public async Task<IActionResult> JustGet()
    //{
    //    var r = await _ttlCache.PeekAsync<WeatherForecast[]?>("WW");
    //    return Ok(r);
    //}

    [HttpGet("cache_same")]
    public async Task<IActionResult> GetSame()
    {
        return await GetDataAsync("Same");
    }

    [HttpGet("cache_new")]
    public async Task<IActionResult> GetNew()
    {
        return await GetDataAsync(Guid.NewGuid().ToString());
    }

    private async Task<IActionResult> GetDataAsync(string cacheKey)
    {
        var sw = Stopwatch.StartNew();

        var r = await _ttlCache.GetAsync(cacheKey, () =>
        {
            var x = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToArray();
            return Task.FromResult(x);
        }, TimeSpan.FromSeconds(100));

        sw.Stop();

        Response.Headers.Add("LoadTime", $"{sw.Elapsed.TotalMilliseconds}ms");

        return Ok(r);
    }
}