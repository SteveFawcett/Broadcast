
using BroadcastPluginSDK.Interfaces;
using CyberDog.Controls;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Loader;

namespace Broadcast.SubForms;

public interface IStartup
{
    public IEnumerable<Assembly> LoadAssemblies();
    public void Hide();
}

public partial class StartUp : Form, IStartup
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly IPluginRegistry? _registry;

    public StartUp(IConfiguration configuration, ILogger logger)
    {
        _configuration = configuration;
        _logger = logger;
        _registry = null;

        InitializeComponent();
        defaultConfiguration();
    }

    public StartUp(IConfiguration configuration, ILogger logger, IPluginRegistry registry)
    {
        _configuration = configuration;
        _logger = logger;
        _registry = registry;

        InitializeComponent();
        defaultConfiguration();
    }

    private void defaultConfiguration()
    {
        if (_configuration == null) return;

        var installPath = _configuration["InstallPath"];

        if (string.IsNullOrEmpty(installPath))
        {
            installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "broadcast", "plugins");
            _configuration["PluginInstallPath"] = installPath;
        }
    }

    public IEnumerable<Assembly> LoadAssemblies()
    {
        List<Assembly> assemblies = [];

        string directory = _configuration["PluginInstallPath"] ?? string.Empty;

        LogPanel.LogDebug($"Using plugin directory: {directory}");

        foreach (var zipPath in Directory.GetFiles(directory, "*.zip"))
        {
            LogPanel.LogDebug($"Found plugin zip at {zipPath}");

            var dllBytesList = ExtractDllsFromZip(zipPath);
            try
            {
                var loadedAssemblies = LoadAssembliesFromBytes(dllBytesList , Path.GetFileName(zipPath));
                SetupAssemblyResolver(loadedAssemblies);

                assemblies.AddRange(loadedAssemblies);
                LogPanel.LogInformation($"Loaded {loadedAssemblies.Count} assemblies from {Path.GetFileName(zipPath)}");
            }
            catch (Exception ex)
            {
                LogPanel.LogError($"Failed to load assemblies from {Path.GetFileName(zipPath)}, Message {ex.Message}");
            }

        }

        return assemblies;
    }

    public new void ShowDialog()
    {
        var text =
            $"{Strings.version}: {Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? Strings.unknown}";
        Show();
        Refresh();

        LogPanel.LogInformation(Strings.start);
        LogPanel.LogDebug(text);
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

    private static List<Assembly> LoadAssembliesFromBytes(List<byte[]> dllBytesList , string name)
    {
        var assemblies = new List<Assembly>();

        foreach (var dllBytes in dllBytesList)
        {
            var context = new PluginLoadContext();

            try
            {
                var assembly = context.LoadFromBytes(dllBytes);

                assemblies.Add(assembly);
            }
            catch (Exception ) 
            {
                LogPanel.LogError($"Failed to load an assembly from: {name}");
            }
        }
        return assemblies;
    }
}

public class PluginLoadContext : AssemblyLoadContext
{
    public PluginLoadContext() : base(isCollectible: false) { }

    public Assembly LoadFromBytes(byte[] dllBytes)
    {
        using var stream = new MemoryStream(dllBytes);
        return LoadFromStream(stream);
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