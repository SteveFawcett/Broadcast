using Microsoft.Extensions.Configuration;


namespace Broadcast
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>

        [STAThread]
        static void Main()
        {
            IConfigurationRoot Configuration;

            var builder = new ConfigurationBuilder()
                    .SetBasePath( Directory.GetCurrentDirectory())
                    .AddJsonFile("settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables(); 

            Configuration = builder.Build();
      
            ApplicationConfiguration.Initialize();
            StartUp StartUp = new StartUp(Configuration); // Initialize the StartUp form and load plugins
            Application.Run(new MainForm( Configuration , StartUp ));
        }

    }
}