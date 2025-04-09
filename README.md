# Tharga Cache

[![GitHub repo Issues](https://img.shields.io/github/issues/Tharga/Cache?style=flat&logo=github&logoColor=red&label=Issues)](https://github.com/Tharga/Cache/issues?q=is%3Aopen)
[![NuGet](https://img.shields.io/nuget/v/Tharga.Cache)](https://www.nuget.org/packages/Tharga.Cache)
![Nuget](https://img.shields.io/nuget/dt/Tharga.Cache)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## Get Started

Register the service
```
var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterCache();
```

Inject to service and start using
```
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
```
