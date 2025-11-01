using Microsoft.Extensions.DependencyInjection;
using Tharga.Cache;
using Tharga.Cache.Console.Commands;
using Tharga.Cache.Core;
using Tharga.Cache.Persist;
using Tharga.Console;
using Tharga.Console.Commands;
using Tharga.Console.Consoles;
using Tharga.Runtime;

var serviceCollection = new ServiceCollection();
_ = AssemblyService.GetTypes<ICommand>().Where(x => !x.IsInterface && !x.IsAbstract).Select(serviceCollection.AddTransient).ToArray();
serviceCollection.AddCache(o =>
{
    o.MaxConcurrentFetchCount = 1;
    o.RegisterType<string, IMemory>(s =>
    {
        s.DefaultFreshSpan = TimeSpan.FromSeconds(10);
        s.StaleWhileRevalidate = true;
        s.MaxSize = 100;
        s.MaxCount = 10;
    });
});
var serviceProvider = serviceCollection.BuildServiceProvider();
serviceProvider.GetService<IWatchDogService>().Start();

using var console = new ClientConsole();
var command = new RootCommand(console, new CommandResolver(type => (ICommand)serviceProvider.GetService(type)));
command.RegisterCommand<CacheCommands>();
var engine = new CommandEngine(command);
engine.Start(args);