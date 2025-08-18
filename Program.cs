using System.Diagnostics;
using Broadcast.SubForms;
using BroadcastPluginSDK;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Broadcast;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        // Build configuration
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        IConfiguration configuration = builder.Build();

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        ILogger logger = loggerFactory.CreateLogger("MSFS");

        // Setup DI container
        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddSingleton<ILoggerFactory>(loggerFactory);
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        services.AddSingleton<IPluginRegistry, PluginRegistry>();
        services.AddTransient<MainForm>();

        // Temporary startup instance to load assemblies before building provider
        var tempStartup = new StartUp(configuration, logger);
        tempStartup.ShowDialog();

        var assemblies = tempStartup.LoadAssemblies();

        // Discover and register plugin types before building provider
        foreach (var assembly in assemblies)
        {
            foreach (var pluginType in DiscoverPluginTypes(assembly))
            {
                services.AddTransient(typeof(IPlugin), pluginType);
            }
        }

        // Build provider after all services are registered
        var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<IPluginRegistry>();

        // Resolve plugin instances via DI
        var plugins = provider.GetServices<IPlugin>();
        foreach (var plugin in plugins)
        {
            registry.Add(plugin);
        }

        tempStartup.Hide();
        var mainForm = provider.GetRequiredService<MainForm>();
        Application.Run(mainForm);
    }

    private static IEnumerable<Type> DiscoverPluginTypes(Assembly assembly)
    {
        var seenTypes = new HashSet<string>();

        foreach (var type in assembly.GetTypes())
        {
            if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            {
                if (type.FullName != null && seenTypes.Add(type.FullName))
                {
                    yield return type;
                }
            }
        }
    }
}
