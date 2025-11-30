//using Tharga.Cache.Web;
//using Xunit;

//namespace Tharga.Cache.Tests.Integration;

//public abstract class TestFixtureBase : IClassFixture<CustomWebApplicationFactory<MemoryData>>
//{
//    protected readonly HttpClient _client;
//    private readonly CustomWebApplicationFactory<MemoryData> _factory;

//    protected TestFixtureBase(CustomWebApplicationFactory<MemoryData> factory)
//    {
//        _factory = factory;
//        factory.ConfigureTestServices = services =>
//        {
//            //services.Remove<IAgentMessageService>();
//        };

//        _client = factory.CreateClient();
//        //var farmService = factory.Services.GetService(typeof(IFarmService));
//    }

//    protected T GetService<T>()
//    {
//        var service = _factory.Services.GetService(typeof(T));
//        return (T)service;
//    }
//}