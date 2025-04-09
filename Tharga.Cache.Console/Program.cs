using Microsoft.Extensions.DependencyInjection;
using Tharga.Cache;
using Tharga.Cache.Console.Commands;
using Tharga.Console;
using Tharga.Console.Commands;
using Tharga.Console.Consoles;
using Tharga.Toolkit.TypeService;

var serviceCollection = new ServiceCollection();
_ = AssemblyService.GetTypes<ICommand>().Where(x => !x.IsInterface && !x.IsAbstract).Select(serviceCollection.AddTransient).ToArray();
serviceCollection.RegisterCache(o =>
{
    //o.DefaultFreshSpan = TimeSpan.FromMinutes(15);
    //o.EvictionPolicy = EvictionPolicy.ARC;

    o.RegisterType<string>(s =>
    {
        s.StaleWhileRevalidate = true;
        s.MaxSize = 100;
        s.MaxCount = 10;
    });

});
var serviceProvider = serviceCollection.BuildServiceProvider();

using var console = new ClientConsole();
var command = new RootCommand(console, new CommandResolver(type => (ICommand)serviceProvider.GetService(type)));
command.RegisterCommand<CacheCommands>();
var engine = new CommandEngine(command);
engine.Start(args);