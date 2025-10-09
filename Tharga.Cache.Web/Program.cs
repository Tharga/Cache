using Quilt4Net.Toolkit.Api;
using Tharga.Cache;
using Tharga.Cache.Persist;
using Tharga.Cache.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCache(o =>
{
    o.MaxConcurrentFetchCount = 1;
    o.RegisterType<WeatherForecast[]?>(s =>
    {
        s.StaleWhileRevalidate = false;
        s.MaxCount = 10;
        s.MaxSize = 2000;
        s.EvictionPolicy = EvictionPolicy.FirstInFirstOut;
        //s.PersistType = PersistType.MemoryWithRedis;
        s.DefaultFreshSpan = TimeSpan.FromSeconds(10);
        //s.PersistType = PersistType.Memory;
        s.PersistType = typeof(IMemory);
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
