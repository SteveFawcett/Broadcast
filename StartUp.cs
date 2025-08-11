using BroadcastPluginSDK;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;

namespace Broadcast
{
    public partial class StartUp : Form
    {
        private IEnumerable<IPlugin> plugins = [];
        private void ShowDialog(IConfigurationRoot Configuration)
        {
            version.Text = $"Version: {Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown"}";

            this.Show();
            this.Refresh();
            
            textBox.Clear();
            textBox.AppendLine($"Starting up Broadcast. Version {version.Text}");
            if (Configuration is not null)
            {
                plugins = ReadDLLs( Configuration, textBox);
            }
            this.Hide();
        }

        public IEnumerable<ICache> Caches()
        {
            foreach (IPlugin plugin in plugins)
            {
                if( plugin is ICache c )
                {
                    yield return c;
                }
            }
        }
        public IEnumerable<IProvider> Providers()
        {
            foreach (IPlugin plugin in plugins)
            {
                if (plugin is IProvider c)
                {
                    yield return c;
                }
            }
        }

        public void AttachTo(Form parent)
        {
            if (parent is MainForm mainForm)
            {
                foreach (IPlugin plugin in plugins)
                {  
                    if (plugin.MainIcon is not null)
                    {
                        textBox.AppendLine($"Adding control {plugin.Name} Version {plugin.Version}. Using configuration {plugin.Stanza}");
                        mainForm.flowLayoutPanel1.Controls.Add(plugin.MainIcon);
                        plugin.Click += mainForm.PluginControl_Click;
                        plugin.MouseHover += mainForm.PluginControl_Hover;
                        plugin.Start();

                        if (plugin is IProvider provider)
                        {
                            Debug.WriteLine($"[1] Plugin {plugin.Name} implements {typeof(IProvider).Name}");
                            provider.DataReceived += mainForm.PluginControl_DataReceived;
                        }

                        if (plugin is ICache cache)
                        {
                            Debug.WriteLine($"[2] Plugin {plugin.Name} implements {typeof(ICache).Name}");
                            //TODO: Implement code to write to cache when data is received
                        }
          
                    }
                }
            }
        }
        public StartUp(IConfigurationRoot Configuration)
        {
            InitializeComponent();
            this.Text = String.Format("Broadcast Start up");
            this.ShowDialog(Configuration);
        }

        static void SetupAssemblyResolver(List<Assembly> loadedAssemblies)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var requestedName = new AssemblyName(args.Name).Name;

                var match = loadedAssemblies.FirstOrDefault(a =>
                    string.Equals(a.GetName().Name, requestedName, StringComparison.OrdinalIgnoreCase));

                return match;
            };
        }
        static public IEnumerable<IPlugin> ReadDLLs( IConfigurationRoot Configuration, TextBox tb)
        {
            List<IPlugin> commands = [];
    
            string directory = Configuration["plugindirectory"] ?? AppDomain.CurrentDomain.BaseDirectory;

            Debug.WriteLine($"Using plugin directory: {directory}");

            foreach (string zipPath in Directory.GetFiles(directory, "*.zip") )
            {
                Debug.WriteLine($"Looking for plugin zip at {zipPath}");

                var dllBytesList = ExtractDllsFromZip(zipPath);
                var loadedAssemblies = LoadAssembliesFromBytes(dllBytesList);
                SetupAssemblyResolver(loadedAssemblies);

                foreach (var assembly in loadedAssemblies)
                {
                    commands.AddRange(CreateCommands(assembly, tb ));
                }
            }

            foreach (IPlugin command in commands)
            {
                tb.AppendLine($"Configuring {command.Name} using stanza {command.Stanza}");
                var section = Configuration.GetSection(command.Stanza);
                if (section == null)
                {
                    tb.AppendLine($"No configuration found for {command.Stanza}. Skipping configuration.");
                    continue;
                }

                if( command.AttachConfiguration( section ) == false )
                {
                    var Dict = new Dictionary<string, string?>();
                    foreach (var child in section.GetChildren())
                    {
                        Dict[child.Key] = child.Value;
                    }
                    command.AttachConfiguration(Dict);
                }

            }

            return commands;
        }


    static List<byte[]> ExtractDllsFromZip(string zipPath)
    {
        var dllBytesList = new List<byte[]>();

        using (var archive = ZipFile.OpenRead(zipPath))
        {
            foreach (var entry in archive.Entries)
            {
                if (entry.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    using (var stream = entry.Open())
                    using (var ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        dllBytesList.Add(ms.ToArray());
                    }
                }
            }
        }

        return dllBytesList;
    }

        static List<Assembly> LoadAssembliesFromBytes(List<byte[]> dllBytesList)
        {
            var assemblies = new List<Assembly>();

            foreach (var dllBytes in dllBytesList)
            {
                var assembly = Assembly.Load(dllBytes);
                assemblies.Add(assembly);
            }

            return assemblies;
        }
        static List<IPlugin> CreateCommands(Assembly assembly, TextBox tb)
        {
            List<IPlugin> commands = [];
            Type[] types = [];

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException)
            {
                Debug.WriteLine($"Partial type load from {assembly.FullName}:");
            }
            foreach ( Type type in types)
            {
                if (typeof(IPlugin).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                {
                    tb.AppendLine($"Found type: {type.FullName} which implements IPlugin");

                    // Ensure the instance is not null and handle potential nullability issues
                    if (Activator.CreateInstance(type) is IPlugin instance)
                    {
                        commands.Add(instance);
                        break;
                    }
                    else
                    {
                        tb.AppendLine($"Failed to create an instance of type: {type.FullName}");
                    }
                }
            }

            return commands;
        }
    }

    public static class WinFormsExtensions
    {
        public static void AppendLine(this TextBox source, string value)
        {
            Debug.WriteLine(value);

            if (source.Text.Length == 0)
                source.Text = value;
            else
                source.AppendText("\r\n" + value);

        }
    }
}
