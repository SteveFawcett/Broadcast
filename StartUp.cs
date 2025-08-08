using Microsoft.Extensions.Configuration;
using PluginBase;
using System.Diagnostics;
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

        static Assembly LoadPlugin(string relativePath, TextBox tb)
        {
            try
            {
                PluginLoadContext loadContext = new( relativePath );
                Assembly assembly = loadContext.LoadFromAssemblyName(AssemblyName.GetAssemblyName( relativePath ));
                Debug.WriteLine($"Loaded {assembly.FullName}");
                return assembly;
            }
            catch (Exception ex)
            {
                tb.AppendLine($"Error loading plugin {relativePath}: {ex.Message}");
                return null!;
            }
        }
        static public IEnumerable<IPlugin> ReadDLLs( IConfigurationRoot Configuration, TextBox tb)
        {
            List<IPlugin> commands = [];
    
            string directory = Configuration["plugindirectory"] ?? AppDomain.CurrentDomain.BaseDirectory;
            
            foreach (IConfigurationSection relativePath in   Configuration.GetSection("plugins").GetChildren() )
            {
                if (relativePath.Value is null || relativePath.Value.Length == 0)
                {
                    tb.AppendLine($"Skipping empty plugin path in configuration.");
                    continue;
                }

                string dll = Path.Combine( directory , relativePath.Value ) + ".dll";
                tb.AppendLine($"Loading plugin from {dll}");

                Assembly assembly = LoadPlugin( dll, tb);
                if (assembly != null)
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


        static List<IPlugin> CreateCommands(Assembly assembly, TextBox tb)
        {
            List<IPlugin> commands = [];
            Type[] types = [];

            try
            {
                types = assembly.GetTypes();
                Debug.WriteLine($"{assembly.FullName} contains {types.Length} types.");
            }
            catch (ReflectionTypeLoadException ex)
            {
                tb.AppendLine($"Error loading types from assembly {assembly.FullName}: {ex.Message}");
                return commands;
            }

            foreach ( Type type in types)
            {
                Debug.WriteLine($"Checking {type.FullName}");

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
