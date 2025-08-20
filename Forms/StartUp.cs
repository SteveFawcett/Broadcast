using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using BroadcastPluginSDK;
using BroadcastPluginSDK.abstracts;
using BroadcastPluginSDK.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
    private readonly IPluginUpdater? _updater;

    public StartUp(IConfiguration configuration, ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;
        _registry = null;

        InitializeComponent();
    }

    public StartUp(IConfiguration configuration, ILogger logger, IPluginRegistry registry, IPluginUpdater? updater)
    {
        _configuration = configuration;
        _logger = logger;
        _registry = registry;
        _updater = updater;

        InitializeComponent();

    }

    public void AddText(string message)
    {
        textBox.AppendLine(message);
        Debug.WriteLine(message);
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
        catch (Exception)
        {
            Debug.WriteLine(value);
        }
    }
}