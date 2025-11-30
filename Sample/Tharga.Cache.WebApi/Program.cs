using Quilt4Net.Toolkit.Api;
using Tharga.Cache;
using Tharga.Cache.MongoDB;
using Tharga.Cache.Persist;
using Tharga.Cache.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCache(o =>
{
    o.Default.DefaultFreshSpan = TimeSpan.FromSeconds(10);
    o.MaxConcurrentFetchCount = 1;
    o.RegisterType<MemoryData, IMemory>();

    o.RegisterType<MongoDBData, IMongoDB>();

    //o.AddRedisDBOptions();
    //o.RegisterType<RedisData, IRedis>();

    o.RegisterType<WeatherForecast[], IMemory>(s =>
    {
        s.StaleWhileRevalidate = false;
        s.MaxCount = 10;
        s.MaxSize = 2000;
        s.EvictionPolicy = EvictionPolicy.FirstInFirstOut;
        s.DefaultFreshSpan = TimeSpan.FromSeconds(10);
    });
});

builder.Services.AddHostedService<CacheMonitorBackgroundService>();

builder.Services.AddQuilt4NetApi(o =>
{
    //o.ShowInOpenApi = !Debugger.IsAttached;
    o.AddComponentService<ComponentService>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseQuilt4NetApi();

app.Run();

public partial class Program { }