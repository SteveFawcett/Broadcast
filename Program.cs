using Broadcast.SubForms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;


namespace Broadcast;

internal static class Program
{
    /// <summary>
    ///     The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("settings.json", true, true)
            .AddEnvironmentVariables();

        IConfiguration configuration = builder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging(config => config.AddConsole());
        services.AddSingleton<IStartup, StartUp>();
        services.AddTransient<MainForm>();

        var provider = services.BuildServiceProvider();
        var mainForm = provider.GetRequiredService<MainForm>();

        Application.Run( mainForm );

    }
}