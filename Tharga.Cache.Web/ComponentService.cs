using Quilt4Net.Toolkit;
using Quilt4Net.Toolkit.Features.Health;

namespace Tharga.Cache.Web;

internal class ComponentService : IComponentService
{
    private readonly ICacheMonitor _cacheMonitor;

    public ComponentService(ICacheMonitor cacheMonitor)
    {
        _cacheMonitor = cacheMonitor;
    }

    public IEnumerable<Component> GetComponents()
    {
        yield return new Component
        {
            Name = "DistributedCache",
            Essential = false,
            CheckAsync = async _ =>
            {
                var result = await _cacheMonitor.GetHealthAsync();
                return new CheckResult
                {
                    Success = result.Success,
                    Message = result.Message
                };
            }
        };
    }
}