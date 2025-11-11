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
        var healthTypes = _cacheMonitor.GetHealthTypes();
        foreach (var healthType in healthTypes)
        {
            yield return new Component
            {
                Name = $"Cache.{healthType.Type}",
                Essential = false,
                CheckAsync = async _ =>
                {
                    var result = await healthType.GetHealthAsync();
                    return new CheckResult
                    {
                        Success = result.Success,
                        Message = result.Message
                    };
                }
            };
        }
    }
}