# Tharga Cache

[![GitHub repo Issues](https://img.shields.io/github/issues/Tharga/Cache?style=flat&logo=github&logoColor=red&label=Issues)](https://github.com/Tharga/Cache/issues?q=is%3Aopen)
[![NuGet](https://img.shields.io/nuget/v/Tharga.Cache)](https://www.nuget.org/packages/Tharga.Cache)
![Nuget](https://img.shields.io/nuget/dt/Tharga.Cache)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## Cache Type Options

When registering a cache type, you can configure the following options:

```csharp
builder.Services.RegisterCache(o =>
{
    o.RegisterType<string, IMemory>(s =>
    {
        s.StaleWhileRevalidate = true;
        s.ReturnDefaultOnFirstLoad = true;
        s.DefaultFreshSpan = TimeSpan.FromSeconds(30);
    });
});
```

- **StaleWhileRevalidate** — When `true`, stale data is returned immediately while fresh data is fetched in the background.
- **ReturnDefaultOnFirstLoad** — When `true`, returns `default(T)` immediately on the first cache miss instead of blocking. The factory runs in the background and populates the cache for subsequent reads. Works independently of `StaleWhileRevalidate`.

> **Note:** `IMemoryWithRedis` is deprecated. Use `IRedis` or `IMemory` explicitly instead.

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
