//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Mvc.Testing;
//using Microsoft.Extensions.DependencyInjection;
//using Tharga.Cache.Web;
//using Tharga.MongoDB;
//using Tharga.Runtime;

//namespace Tharga.Cache.MongoDB.Tests;

//public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
//{
//    public Action<IServiceCollection> ConfigureTestServices { get; set; }

//    protected override void ConfigureWebHost(IWebHostBuilder builder)
//    {
//        builder.ConfigureServices(services =>
//        {
//            services.AddMongoDB(o =>
//            {
//                o.AutoRegistrationAssemblies = AssemblyService.GetAssemblies<MemoryData>();
//            });
//        });
//    }
//}