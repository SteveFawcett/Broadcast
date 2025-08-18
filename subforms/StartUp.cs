using BroadcastPluginSDK;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using static Broadcast.SubForms.MainForm;

namespace Broadcast.SubForms;


public interface IStartup
{
    public IEnumerable<Assembly> LoadAssemblies();
    public void Hide();
    public void AddText(string message);
}

public partial class StartUp : Form, IStartup
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly IPluginRegistry? _registry; 

    public void AddText( string message)
    {
        textBox.AppendLine(message);
        Debug.WriteLine(message);
    }

    public StartUp(IConfiguration configuration , ILogger logger )
    {
        _configuration = configuration;
        _logger = logger;
        _registry = null;

        InitializeComponent();
    }

    public StartUp(IConfiguration configuration, ILogger logger, IPluginRegistry registry)
    {
        _configuration = configuration;
        _logger = logger;
        _registry = registry;

        InitializeComponent();
        AttachMasterReader();
    }
    public new void ShowDialog()
    {
        var text =
            $"{Strings.version}: {Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? Strings.unknown}";
        Show();
        Refresh();

        textBox.Clear();
        AddText(Strings.start);
        AddText(text);

    }

    public void AttachMasterReader()
    {
        if (_registry is null)
        {
            AddText("No registry available, skipping master reader attachment.");
            return;
        }
        var master = _registry.MasterCache();

        if (master is null) return;

        if (master is BroadcastCacheBase c)
        {
            foreach (var plugin in _registry.GetAll())
            {
                //AddText($"Attaching master reader to {plugin.Name} => {c.Name}");
                plugin.GetCacheData = c.CacheReader;
            }
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

    public IEnumerable<Assembly> LoadAssemblies()
    {
        List<Assembly> assemblies = [];

        var directory = _configuration["plugindirectory"] ?? AppDomain.CurrentDomain.BaseDirectory;

        AddText($"Using plugin directory: {directory}");

        foreach (var zipPath in Directory.GetFiles(directory, "*.zip"))
        {
            AddText($"Looking for plugin zip at {zipPath}");

            var dllBytesList = ExtractDllsFromZip(zipPath);
            var loadedAssemblies = LoadAssembliesFromBytes(dllBytesList);
            SetupAssemblyResolver(loadedAssemblies);

            assemblies.AddRange(loadedAssemblies);
        }

        return assemblies;
    }

    /***
    foreach (var command in commands)
        {
            tb.AppendLine($"Configuring {command.Name} using stanza {command.Stanza}");
            var section = _configuration.GetSection(command.Stanza);
            
            if (section == null)
            {
                tb.AppendLine($"No configuration found for {command.Stanza}. Skipping configuration.");
                continue;
            }

            if (command.AttachConfiguration(section) == false)
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

    ****/

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

}

public static class WinFormsExtensions
{
    public static void AppendLine(this TextBox source, string value)
    {
        try
        {
            if (source.Text.Length == 0)
                source.Text = value;
            else
                source.AppendText("\r\n" + value);
        }
        catch (Exception )
        {
            Debug.WriteLine( value);
        }

    }
}