using Quilt4Net.Toolkit.Api;
using Tharga.Cache;
using Tharga.Cache.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.RegisterCache(o =>
{
    //TODO: This in combination with Peak will increase performance.
    o.RegisterType<WeatherForecast[]?>(s =>
    {
        s.StaleWhileRevalidate = true;
        //s.MaxCount = 3;
        s.MaxSize = 2000;
        s.EvictionPolicy = EvictionPolicy.FirstInFirstOut;
    });
});

builder.Services.AddHostedService<CacheMonitorBackgroundService>();

builder.Services.AddQuilt4NetApi();

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
