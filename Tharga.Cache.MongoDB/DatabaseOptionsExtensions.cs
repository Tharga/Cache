using System.Reflection;
using Tharga.MongoDB.Configuration;

namespace Tharga.Cache.MongoDB;

public static class DatabaseOptionsExtensions
{
    public static void AddCache(this DatabaseOptions options)
    {
        options.AddAutoRegistrationAssembly(Assembly.GetAssembly(typeof(IMongoDB)));
    }
}