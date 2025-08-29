using Broadcast.Classes;
using Broadcast.SubForms;
using BroadcastPluginSDK.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;

namespace Broadcast.Classes
{
    public interface ILocalConfigurationManager
    {
        public void Save();
    }
    public class LocalConfigurationManager : ILocalConfigurationManager
    {
        IConfiguration _configuration;
        IPluginRegistry _registry;
        ILogger<IPlugin> _logger;
        string pluginPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Broadcast\";

        public LocalConfigurationManager( IConfiguration configuration , ILogger<IPlugin> logger , IPluginRegistry registry)
        {
            _configuration = configuration;
            _logger = logger;
            _registry = registry;
            _logger.LogInformation("Local Configuration Manager initialized");
        }

        public void Save( )
        {
            var file = Path.Combine(pluginPath, "settings.json");
            string json = WriteSelectedConfigToJson(_configuration , GetKeys() );
            File.WriteAllText(file, json);
            _logger.LogInformation("Configuration saved to {path}", file);
        }

        public List<string> GetKeys()
        {
            var keys = new List<string>
                        {
                            "RepositoryUrl",
                            "LogLevel",
                            "PluginInstallPath"
                        };

            foreach( IPlugin p in _registry.GetAll() )
            {
                if( string.IsNullOrEmpty( p.Stanza) ) continue;

                _logger.LogInformation("Getting {Stanza} keys for plugin {Name}", p.Stanza , p.Name);
                keys.Add(p.Stanza);
            }
            return keys;
        }

        public static string WriteSelectedConfigToJson(IConfiguration config, IEnumerable<string> keysToInclude)
        {
            var filtered = new Dictionary<string, object>();

            foreach (var key in keysToInclude)
            {
                var section = config.GetSection(key);

                // Handle top-level keys (no children, just a value)
                if (!section.GetChildren().Any())
                {
                    filtered[key] = section.Value ?? string.Empty;
                }
                else
                {
                    // Handle nested sections
                    var nested = new Dictionary<string, object>();
                    Populate(section, nested);
                    filtered[key] = nested;
                }
            }

            return JsonSerializer.Serialize(filtered, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        private static void Populate(IConfiguration section, IDictionary<string, object> target)
        {
            foreach (var child in section.GetChildren())
            {
                if (child.GetChildren().Any())
                {
                    var nested = new Dictionary<string, object>();
                    Populate(child, nested);
                    target[child.Key] = nested;
                }
                else
                {
                    target[child.Key] = child.Value ?? string.Empty;
                }
            }
        }

    }
}
