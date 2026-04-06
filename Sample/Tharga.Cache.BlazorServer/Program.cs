using Radzen;
using Tharga.Cache;
using Tharga.Cache.BlazorServer.Components;
using Tharga.Cache.BlazorServer.Components.Pages;
using Tharga.Cache.MongoDB;
using Tharga.Cache.Persist;
using Tharga.MongoDB;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddRadzenComponents();
builder.AddMongoDB();
builder.Services.AddCache(o =>
{
    o.RegisterType<string, IMemory>(s =>
    {
        s.DefaultFreshSpan = TimeSpan.FromSeconds(3);
    });

    o.RegisterType<Weather.WeatherForecast[], IMongoDB>(s =>
    {
        s.DefaultFreshSpan = TimeSpan.FromSeconds(60);
        s.StaleWhileRevalidate = true;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
