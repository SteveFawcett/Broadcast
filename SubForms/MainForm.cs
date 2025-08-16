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
    private readonly IStartup _plugins;

    public MainForm(IConfiguration configuration, ILogger<MainForm> logger , IStartup plugins)
    {
        _configuration = configuration;
        _logger = logger;
        _plugins = plugins;

        InitializeComponent();
        toolStripStatusLabel.Text = Strings.PluginStarting;

        _plugins.AttachTo(this);

        //_plugins.AttachMasterReader();
    }

    public void PluginControl_Click(object? sender, EventArgs e)
    {
        Debug.WriteLine($"PluginControl_Click {sender?.GetType().Name}");
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
        foreach (var plugin in _plugins.Caches() ?? [])
        {
            if (plugin is not ICache c) continue;
            c.Write(e);
        }
    }

    private void CheckForUpdates(object sender, EventArgs e)
    {
        Debug.WriteLine("Checking for updates...");
        UpdateForm updateForm = new(_plugins.All());
        updateForm.ShowDialog(this);
    }

}