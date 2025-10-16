using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Tharga.MongoDB;
using Tharga.Toolkit.TypeService;

namespace Tharga.Cache.MongoDB.Tests;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    public Action<IServiceCollection> ConfigureTestServices { get; set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddMongoDB(o =>
            {
                o.AutoRegistrationAssemblies = AssemblyService.GetAssemblies<Program>();
            });
        });
    }
}