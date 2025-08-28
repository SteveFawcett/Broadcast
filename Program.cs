using System.Reflection;
using Broadcast.SubForms;
using Broadcast.Classes;
using BroadcastPluginSDK.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Broadcast;

internal static class Program
{

    [STAThread]
    private static void Main()
    {
        // Build configuration
        string pluginPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Broadcast\";

        var builder = new ConfigurationBuilder()
            .SetBasePath( pluginPath )
            .AddJsonFile("settings.json", true, true)
            .AddEnvironmentVariables();

        IConfiguration configuration = builder.Build();
        var loglevelString = configuration["loglevel"] ?? "Information";
        var loglevel = Enum.TryParse<LogLevel>(loglevelString, true, out var parsedLevel)
            ? parsedLevel
            : LogLevel.Information;

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel( loglevel);
            builder.AddDebug();
        });

        var logger = loggerFactory.CreateLogger("MSFS");
        logger.LogInformation("Application starting...");


        // Setup DI container
        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddSingleton<ILocalConfigurationManager, LocalConfigurationManager>() ;
        services.AddSingleton<ILoggerFactory>(loggerFactory);
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        services.AddSingleton<IPluginRegistry, PluginRegistry>();
        services.AddTransient<MainForm>();

        // Temporary startup instance to load assemblies before building provider
        var tempStartup = new StartUp(configuration, logger);
        tempStartup.ShowDialog();

        var assemblies = tempStartup.LoadAssemblies();

        StartUp.AddText("Discover and register plugin types before building provider");
        foreach (var assembly in assemblies)
        {
            logger.LogDebug($"Scanning assembly: {assembly.FullName}");
            try
            {
                foreach (var pluginType in DiscoverPluginTypes(assembly))
                {
                    try
                    {
                        StartUp.AddText($"Registering plugin: {pluginType.FullName}");
                        services.AddTransient(typeof(IPlugin), pluginType);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Failed to register plugin type: {pluginType.FullName}");
                    }
                }
            }
            catch (ReflectionTypeLoadException rtle)
            {
                foreach (var loaderException in rtle.LoaderExceptions)
                {
                    logger.LogError(loaderException, $"Loader exception in assembly: {assembly.FullName}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to scan assembly: {assembly.FullName}");
            }
        }

        // Build provider after all services are registered
        var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<IPluginRegistry>();

        // Resolve plugin instances via DI
        var plugins = provider.GetServices<IPlugin>();
        foreach (var plugin in plugins) registry.Add(plugin);

        tempStartup.Hide();
        registry.AttachMasterReader();

        var mainForm = provider.GetRequiredService<MainForm>();
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.Run(mainForm);
    }

    private static IEnumerable<Type> DiscoverPluginTypes(Assembly assembly)
    {
        var seenTypes = new HashSet<string>();

        foreach (var type in assembly.GetTypes())
            if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                if (type.FullName != null && seenTypes.Add(type.FullName))
                    yield return type;
    }
}