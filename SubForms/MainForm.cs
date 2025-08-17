using BroadcastPluginSDK;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Broadcast.SubForms;

public partial class MainForm : Form
{
    public delegate IEnumerable<KeyValuePair<string, string>> CacheReader(List<string> values);
    private readonly IConfiguration _configuration;
    private readonly ILogger<MainForm> _logger;
    private readonly IStartup _startup;

    public ILogger Logger => _logger;
    public IConfiguration Configuration => _configuration;
    public IStartup Startup => _startup;
    
    public MainForm(IConfiguration configuration, ILogger<MainForm> logger , IStartup startup)
    {
        _configuration = configuration;
        _logger = logger;
        _startup = startup;

        InitializeComponent();
        toolStripStatusLabel.Text = Strings.PluginStarting;

        AttachToForm();

        //_startup.AttachMasterReader();
    }

    public void PluginControl_Click(object? sender, EventArgs e)
    {
        Logger.LogDebug($"PluginControl_Click {sender?.GetType().Name}");
        if (sender is IPlugin c)
        {
            panel.Controls.Clear();
            panel.Controls.Add(c.InfoPage ?? new InfoPage());
            panel.Size = c.InfoPage?.Size ?? new Size(300, 300);
        }
    }

    internal void PluginControl_Hover(object? sender, EventArgs e)
    {
        if (sender is IPlugin c) toolStripStatusLabel.Text = $"{c.Name} ({c.Version}) : {c.Description}";
    }

    internal void PluginControl_DataReceived(object? sender, Dictionary<string, string> e)
    {
        foreach (var plugin in _startup.Caches() ?? [])
        {
            if (plugin is not ICache c) continue;
            c.Write(e);
        }
    }

    private void CheckForUpdates(object sender, EventArgs e)
    {
        Logger.LogDebug("Checking for updates...");
        UpdateForm updateForm = new(_startup.All());
        updateForm.ShowDialog(this);
    }

    public void AttachToForm()
    {
        foreach (var plugin in _startup.Plugins)
        {
            flowLayoutPanel1.Controls.Add(plugin.MainIcon);
            plugin.Click += PluginControl_Click;
            plugin.MouseHover += PluginControl_Hover;

            var res = plugin.Start();
            if (!string.IsNullOrWhiteSpace(res)) toolStripStatusLabel.Text = res;

            if (plugin is IProvider provider)
            {
                Logger.LogDebug($"[1] Plugin {plugin.Name} implements {nameof(IProvider)}");
                provider.DataReceived += PluginControl_DataReceived;
                
                if (plugin is ICache cache) Logger.LogDebug($"[2] Plugin {plugin.Name} implements {nameof(ICache)}");
            }

        }
    }
}