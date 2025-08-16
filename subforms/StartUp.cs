using BroadcastPluginSDK;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using Microsoft.Extensions.Logging;
using static Broadcast.SubForms.MainForm;

namespace Broadcast.SubForms;

public interface IStartup
{
    public void AttachTo(Form parent);
    public IEnumerable<BroadcastCacheBase>? Caches();
    public IEnumerable<Dictionary<string, string>> All();
}

public partial class StartUp : Form, IStartup
{
    private IEnumerable<IPlugin> _plugins = [];
    private readonly IConfiguration _configuration;
    private readonly ILogger<MainForm> _logger;

    public StartUp(IConfiguration configuration , ILogger<MainForm> logger )
    {
        _configuration = configuration;
        _logger = logger;
        InitializeComponent();
        ShowDialog();
    }

    public new void ShowDialog()
    {
        var text =
            $"{Strings.version}: {Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? Strings.unknown}";
        Show();
        Refresh();

        textBox.Clear();
        textBox.AppendLine(Strings.start);
        textBox.AppendLine(text);

        _plugins = ReadDLLs(_configuration, textBox);

        Hide();
    }

    public BroadcastCacheBase? MasterCache()
    {
        foreach (var c in Caches())
        {
            Debug.WriteLine($"Resolved type: {c.GetType().Name}");

            if (c.Master)
            {
                Debug.WriteLine($"Found master cache: {c.Name}");
                return c;
            }
        }

        Debug.WriteLine("No master cache found.");
        return null;
    }

    public IEnumerable<BroadcastCacheBase>? Caches()
    {
        foreach (var plugin in _plugins)
            if (plugin is BroadcastCacheBase c)
                yield return c;
    }

    public IEnumerable<IProvider>? Providers()
    {
        foreach (var plugin in _plugins)
            if (plugin is IProvider c)
                yield return c;
    }

    public void AttachMasterReader()
    {
        var master = MasterCache();

        if (master is null) return;

        if (master is BroadcastCacheBase c)
        {
            foreach (var plugin in _plugins)
            {
                Debug.WriteLine($"Attaching master reader to {plugin.Name} => {c.Name}");
                plugin.GetCacheData = c.CacheReader;
            }
        }
    }

    public IEnumerable<Dictionary<string, string>> All()
    {
        foreach (var plugin in _plugins)
        {
            Dictionary<string, string> c = new()
            {
                { "Name", plugin.Name },
                { "Version", plugin.Version },
                { "FilePath", plugin.FilePath },
                { "Description", plugin.Description },
                { "RepositoryUrl", plugin.RepositoryUrl }
            };

            yield return c;
        }
    }

    public void AttachTo(Form parent)
    {
        if (parent is MainForm mainForm)
            foreach (var plugin in _plugins)
            {
                mainForm.flowLayoutPanel1.Controls.Add(plugin.MainIcon);
                plugin.Click += mainForm.PluginControl_Click;
                plugin.MouseHover += mainForm.PluginControl_Hover;

                var res = plugin.Start();
                if (!string.IsNullOrWhiteSpace(res)) mainForm.toolStripStatusLabel.Text = res;

                if (plugin is IProvider provider)
                {
                    Debug.WriteLine($"[1] Plugin {plugin.Name} implements {nameof(IProvider)}");
                    provider.DataReceived += mainForm.PluginControl_DataReceived;
                }

                if (plugin is ICache cache) Debug.WriteLine($"[2] Plugin {plugin.Name} implements {nameof(ICache)}");
                //TODO: Implement code to write to cache when data is received
            }
    }

    private static void SetupAssemblyResolver(List<Assembly> loadedAssemblies)
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            var requestedName = new AssemblyName(args.Name).Name;

            var match = loadedAssemblies.FirstOrDefault(a =>
                string.Equals(a.GetName().Name, requestedName, StringComparison.OrdinalIgnoreCase));

            return match;
        };
    }

    public static IEnumerable<IPlugin> ReadDLLs(IConfiguration configuration, TextBox tb)
    {
        List<IPlugin> commands = [];

        var directory = configuration["plugindirectory"] ?? AppDomain.CurrentDomain.BaseDirectory;

        Debug.WriteLine($"Using plugin directory: {directory}");

        foreach (var zipPath in Directory.GetFiles(directory, "*.zip"))
        {
            Debug.WriteLine($"Looking for plugin zip at {zipPath}");

            var dllBytesList = ExtractDllsFromZip(zipPath);
            var loadedAssemblies = LoadAssembliesFromBytes(dllBytesList);
            SetupAssemblyResolver(loadedAssemblies);

            foreach (var assembly in loadedAssemblies) commands.AddRange(CreateCommands(assembly, tb, zipPath));
        }

        foreach (var command in commands)
        {
            tb.AppendLine($"Configuring {command.Name} using stanza {command.Stanza}");
            var section = configuration.GetSection(command.Stanza);
            if (section == null)
            {
                tb.AppendLine($"No configuration found for {command.Stanza}. Skipping configuration.");
                continue;
            }

            if (command.AttachConfiguration(section) == false)
            {
                var dict = new Dictionary<string, string?>();
                foreach (var child in section.GetChildren()) dict[child.Key] = child.Value;
                command.AttachConfiguration(dict);
            }
        }

        return commands;
    }


    private static List<byte[]> ExtractDllsFromZip(string zipPath)
    {
        var dllBytesList = new List<byte[]>();

        using (var archive = ZipFile.OpenRead(zipPath))
        {
            foreach (var entry in archive.Entries)
                if (entry.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    using var stream = entry.Open();
                    using var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    dllBytesList.Add(ms.ToArray());
                }
        }

        return dllBytesList;
    }

    private static List<Assembly> LoadAssembliesFromBytes(List<byte[]> dllBytesList)
    {
        var assemblies = new List<Assembly>();

        foreach (var dllBytes in dllBytesList)
        {
            var assembly = Assembly.Load(dllBytes);
            assemblies.Add(assembly);
        }

        return assemblies;
    }

    private static List<IPlugin> CreateCommands(Assembly assembly, TextBox tb, string filePath)
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

        foreach (var type in types)
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

                tb.AppendLine($"Failed to create an instance of type: {type.FullName}");
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