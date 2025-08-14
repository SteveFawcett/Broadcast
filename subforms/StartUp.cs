using BroadcastPluginSDK;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;

namespace Broadcast.SubForms
{
    public partial class StartUp : Form
    {
        private IEnumerable<IPlugin> _plugins = [];
        private void ShowDialog(IConfigurationRoot configuration)
        {
            var text = $"{Strings.version}: {Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? Strings.unknown }";
            Show();
            Refresh();
            
            textBox.Clear();
            textBox.AppendLine(Strings.start);
            textBox.AppendLine(text);

            _plugins = ReadDLLs( configuration, textBox);

            Hide();
        }

        public ICache? MasterCache()
        {
            var plugins = Caches();
            if (plugins == null || !plugins.Any())
            {
                Debug.WriteLine($"No plugins of type {nameof(ICache)} have been loaded");
                return null;
            }

            var masterCaches = plugins.Where(p => p.Master ).ToList();

            if (masterCaches.Count > 1)
            {
                if (masterCaches.First() is IPlugin c)
                {
                    Debug.WriteLine(
                        $"More than one plugin is marked as Master; returning the first. [{c.Name}]");
                }
            }
            else if (masterCaches.Count == 0)
            {
                Debug.WriteLine("No plugins are marked as Master.");
                return null;
            }

            return masterCaches.First();
        }

        public IEnumerable<ICache>? Caches()
        {
            foreach (IPlugin plugin in _plugins)
            {
                if( plugin is ICache c )
                {
                    yield return c;
                }
            }
        }
        public IEnumerable<IProvider>? Providers()
        {
            foreach (IPlugin plugin in _plugins)
            {
                if (plugin is IProvider c)
                {
                    yield return c;
                }
            }
        }

        public IEnumerable<Dictionary<string,string>> All()
        {
            foreach (IPlugin plugin in _plugins)
            {
                Dictionary<string, string> c = new()
                {
                    { "Name"          , plugin.Name },
                    { "Version"       , plugin.Version },
                    { "FilePath"      , plugin.FilePath },
                    { "Description"   , plugin.Description },
                    { "RepositoryUrl" , plugin.RepositoryUrl }
                };

                yield return c;
            }
        }

        public void AttachTo(Form parent)
        {
            if (parent is MainForm mainForm)
            {
                foreach (IPlugin plugin in _plugins)
                {
                    mainForm.flowLayoutPanel1.Controls.Add(plugin.MainIcon);
                    plugin.Click += mainForm.PluginControl_Click;
                    plugin.MouseHover += mainForm.PluginControl_Hover;
                    string res = plugin.Start();
                    if (!string.IsNullOrWhiteSpace(res))
                    {
                        mainForm.toolStripStatusLabel.Text = res;
                    }

                    if (plugin is IProvider provider)
                    {
                        Debug.WriteLine($"[1] Plugin {plugin.Name} implements {nameof(IProvider)}");
                        provider.DataReceived += mainForm.PluginControl_DataReceived;
                    }

                    if (plugin is ICache cache)
                    {
                        Debug.WriteLine($"[2] Plugin {plugin.Name} implements {nameof(ICache)}");
                        //TODO: Implement code to write to cache when data is received
                    }
                        
                }
            }
        }
        public StartUp(IConfigurationRoot configuration)
        {
            InitializeComponent();
            ShowDialog(configuration);
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
        public static IEnumerable<IPlugin> ReadDLLs( IConfigurationRoot configuration, TextBox tb)
        {
            List<IPlugin> commands = [];
    
            string directory = configuration["plugindirectory"] ?? AppDomain.CurrentDomain.BaseDirectory;

            Debug.WriteLine($"Using plugin directory: {directory}");

            foreach (string zipPath in Directory.GetFiles(directory, "*.zip") )
            {
                Debug.WriteLine($"Looking for plugin zip at {zipPath}");

                var dllBytesList = ExtractDllsFromZip(zipPath);
                var loadedAssemblies = LoadAssembliesFromBytes(dllBytesList);
                SetupAssemblyResolver(loadedAssemblies);

                foreach (var assembly in loadedAssemblies)
                {
                    commands.AddRange(CreateCommands(assembly, tb , zipPath ));
                }
            }

            foreach (IPlugin command in commands)
            {
                tb.AppendLine($"Configuring {command.Name} using stanza {command.Stanza}");
                var section = configuration.GetSection(command.Stanza);
                if (section == null)
                {
                    tb.AppendLine($"No configuration found for {command.Stanza}. Skipping configuration.");
                    continue;
                }

                if( command.AttachConfiguration( section ) == false )
                {
                    var dict = new Dictionary<string, string?>();
                    foreach (var child in section.GetChildren())
                    {
                        dict[child.Key] = child.Value;
                    }
                    command.AttachConfiguration(dict);
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
                    using var stream = entry.Open();
                    using var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    dllBytesList.Add(ms.ToArray());
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
        static List<IPlugin> CreateCommands(Assembly assembly, TextBox tb , string filePath)
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
                        instance.FilePath = filePath;
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
