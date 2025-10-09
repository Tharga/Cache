using System.Diagnostics;
using Quilt4Net.Toolkit.Api;
using Tharga.Cache;
using Tharga.Cache.MongoDB;
using Tharga.Cache.Web;
using Tharga.MongoDB;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//TODO: When adding AddMongoDB before the Cache, the cache-collection cannot be found.
//This is due to dynamic registration of the cache. If we want this to work, the AddMongoDB should not perform the registration, it should be done with "UseMongoDB" (that should also be executed automatically, after the setup)
//builder.Services.AddMongoDB(o =>
//{
//    o.ConnectionStringLoader = (s, e) =>
//    {
//        Debugger.Break();
//        throw new NotImplementedException();
//    };
//});
builder.Services.AddCache(o =>
{
    o.MaxConcurrentFetchCount = 1;
    o.AddMongoDBOptions(e =>
    {
        //e.CollectionName = "config-from-code";
        //e.ConfigurationName = "Yee";
    });
    o.RegisterType<WeatherForecast[]?, IMongoDB>(s =>
    {
        s.StaleWhileRevalidate = false;
        s.MaxCount = 10;
        s.MaxSize = 2000;
        s.EvictionPolicy = EvictionPolicy.FirstInFirstOut;
        s.DefaultFreshSpan = TimeSpan.FromSeconds(10);
    });
});
builder.Services.AddMongoDB(o =>
{
    //o.ConnectionStringLoader = (s, e) =>
    //{
    //    Debugger.Break();
    //    throw new NotImplementedException();
    //};
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
