using Broadcast.SubForms;
using Microsoft.Extensions.Configuration;

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

        ApplicationConfiguration.Initialize();
        StartUp startUp = new(configuration); // Initialize the StartUp form and load plugins
        Application.Run(new MainForm(startUp));
    }
}