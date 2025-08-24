using BroadcastPluginSDK.abstracts;
using BroadcastPluginSDK.Interfaces;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Runtime.Loader;

namespace Broadcast.Classes;

public class PluginRegistry : IPluginRegistry 
{
    //   public IEnumerable<PluginInfo> GetPluginInfo() =>
    //       _plugins.Select(p => new PluginInfo(p.Name, p.Version, p.FilePath, p.Description, p.RepositoryUrl));

    private readonly List<IPlugin> _plugins = new();

    private readonly ILogger<PluginRegistry> _logger;
    public PluginRegistry(ILogger<PluginRegistry> logger)
    {
        _logger = logger;
        _logger.LogInformation("PluginRegistry initialized.");
    }

    public IPlugin? Get(string shortname)
    {
        var plugin = _plugins.FirstOrDefault(p => p.ShortName.Equals(shortname, StringComparison.OrdinalIgnoreCase));
        if (plugin != null)
        {
            _logger.LogDebug($"Searching (Found) for plugin with shortname: {shortname}");
            return plugin;
        }
        _logger.LogDebug($"Searching (Not Found) for plugin with shortname: {shortname}");
        return null;
    }
    public void Add(IPlugin plugin)
    {
        _logger.LogDebug($"Adding plugin to registry: {plugin.ShortName} (Version: {plugin.Version})");
        _plugins.Add(plugin);
    }

    public IReadOnlyList<IPlugin> GetAll() => _plugins.AsReadOnly();

    public ICache? MasterCache()
    {
        var c = Caches()?.FirstOrDefault(c => c.Master);
        
        if (c != null)
        {
            _logger.LogInformation($"Master cache found: {c.Name}");
            return c;
        }
       
        c = Caches()?.FirstOrDefault();
        if (c != null)
        {
            _logger.LogInformation($"No master cache found. Choosing first cache {c.Name}");
            return c;
        }

        _logger.LogError("No caches found.");
        return null;
    }

    public IEnumerable<BroadcastCacheBase>? Caches()
    {
        foreach (var plugin in _plugins)
            if (plugin is ICache c)
                yield return (BroadcastCacheBase)plugin;
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
            foreach (var plugin in GetAll())
            {
                _logger.LogDebug($"Attaching cache reader to plugin: {plugin.Name}");
                plugin.GetCacheData = c.CacheReader;
            }
    }
    public record PluginInfo(string Name, string Version, string FilePath, string Description, string RepositoryUrl);
}