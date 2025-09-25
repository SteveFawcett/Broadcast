using Broadcast.Classes;
using Broadcast.SubForms;
using BroadcastPluginSDK.Interfaces;
using CyberDog.Controls;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using System.Diagnostics;
using System.Reflection;

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

        var log = "Application";
        var source = "Broadcast";

        if (!EventLog.SourceExists(source))
        {
            EventLog.CreateEventSource(source, log );
        }

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.AddEventLog(settings =>
            {
                settings.LogName = log;      // Or a custom log name
                settings.SourceName = source;    // Must be registered in Event Viewer
            });
            builder.SetMinimumLevel( loglevel);
            builder.AddDebug();
        });


        // Setup DI container
        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddSingleton<ILocalConfigurationManager, LocalConfigurationManager>() ;
        services.AddSingleton<ILoggerFactory>(loggerFactory);
        services.AddSingleton(typeof(ILogger<>), typeof(ContextualLogger<>));
        services.AddSingleton<IPluginRegistry, PluginRegistry>();
        services.AddTransient<MainForm>();

        // Temporary startup instance to load assemblies before building provider
        var tempProvider = services.BuildServiceProvider();
        var logger = new ContextualLogger(tempProvider.GetRequiredService<ILoggerFactory>().CreateLogger("BROADCAST"));

        var tempStartup = new StartUp(configuration, logger);
        tempStartup.ShowDialog();

        var assemblies = tempStartup.LoadAssemblies();

        StartUp.LogPanel.LogInformation("Discover and register plugin types before building provider");

        foreach (var assembly in assemblies)
        {
            StartUp.LogPanel.LogDebug($"Scanning assembly: {assembly.FullName}");

            StartUp.LogPanel.LogDebug($"Plugin interface type: {typeof(IPlugin).Assembly.FullName}");
            
            try
            {
                var pluginTypes = assembly.GetTypes()
                    .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass);

                foreach (var type in pluginTypes)
                {
                    StartUp.LogPanel.LogDebug($"Type implements IPlugin: {typeof(IPlugin).IsAssignableFrom(type)}");

                    try
                    {
                        var pluginInstance = (IPlugin)Activator.CreateInstance(type)!;

                        StartUp.LogPanel.LogInformation($"Registering plugin: {pluginInstance.Name}");
                        services.AddTransient(typeof(IPlugin), type);
                    }
                    catch (Exception ex)
                    {
                        StartUp.LogPanel.LogError( $"Failed to register plugin type: {type.FullName} Message: {ex.Message}");
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (var loaderEx in ex.LoaderExceptions)
                {
                    StartUp.LogPanel.LogError( $"Type load error in assembly: {assembly.FullName} , message: {loaderEx!.Message}");
                }
            }
            catch (Exception ex)
            {
                StartUp.LogPanel.LogError($"Failed to scan assembly: {assembly.FullName} , message: {ex.Message}");
            }
        }


        // Build provider after all services are registered
        var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<IPluginRegistry>();

        // Resolve plugin instances via DI
        var plugins = provider.GetServices<IPlugin>();
        StartUp.LogPanel.LogInformation($"Total plugins discovered: {plugins.Count()}");

        foreach (var plugin in plugins)
        {
            registry.Add(plugin);
        }

        tempStartup.Hide();
        registry.AttachMasterReader();

        var mainForm = provider.GetRequiredService<MainForm>();
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.Run(mainForm);
    }

}