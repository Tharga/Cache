using Microsoft.AspNetCore.Mvc;

namespace Tharga.Cache.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ITimeToLiveCache _ttlCache;

    public WeatherForecastController(ITimeToLiveCache ttlCache)
    {
        _ttlCache = ttlCache;
    }

    [HttpGet]
    public async Task<IActionResult> GetSame()
    {
        var response = await _ttlCache.GetAsync("MyCacheKey", LoadWeatherData, TimeSpan.FromSeconds(30));
        return Ok(response);
    }

    private static Task<WeatherForecast[]> LoadWeatherData()
    {
        var data = Enumerable.Range(1, 5).Select(index => new WeatherForecast { Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)), TemperatureC = Random.Shared.Next(-20, 55) }).ToArray();
        return Task.FromResult(data);
    }
}