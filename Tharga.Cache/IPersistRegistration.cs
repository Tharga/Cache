using Microsoft.Extensions.DependencyInjection;

namespace Tharga.Cache;

public interface IPersistRegistration
{
    void Register(IServiceCollection services);
}