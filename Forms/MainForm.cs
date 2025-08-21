using System.Diagnostics;
using BroadcastPluginSDK;
using BroadcastPluginSDK.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Broadcast.SubForms;

public partial class MainForm : Form
{
    public delegate IEnumerable<KeyValuePair<string, string>> CacheReader(List<string> values);

    private readonly ILogger<IPlugin> _logger;
    private readonly IPluginRegistry _registry;
    private readonly IConfiguration _configuration;
    private readonly IPluginUpdater _updater;

    public ILogger Logger => _logger;
    
    public MainForm(IConfiguration configuration, ILogger<IPlugin> logger, IPluginRegistry registry, IPluginUpdater updates)
    {
        _configuration = configuration;
        _logger = logger;
        _registry = registry;
        _updater = updates;

        InitializeComponent();
        toolStripStatusLabel.Text = Strings.PluginStarting;

        AttachToForm();
    }

    // Update the code in MainForm to use the new method:
    public void PluginControl_Click(object? sender, EventArgs e)
    {
        Logger.LogDebug($"PluginControl_Click {sender?.GetType().Name}");
        if (sender is IPlugin c)
        {
            panel.Controls.Clear();
            panel.Controls.Add(c.InfoPage.GetControl()); // Use the GetControl method to add the Control.
        }
    }

    internal void PluginControl_Hover(object? sender, EventArgs e)
    {
        if (sender is IPlugin c) toolStripStatusLabel.Text = $"{c.Name}";
    }

    internal void PluginControl_DataReceived(object? sender, Dictionary<string, string> e)
    {
        foreach (var plugin in _registry.Caches() ?? [])
        {
            if (plugin is ICache c) c.Write(e);
        }
    }

    private void CheckForUpdates(object sender, EventArgs e)
    {
        UpdateForm updateForm = new(_logger, _updater);
        updateForm.ShowDialog(this);
    }

    public void AttachToForm()
    {
        foreach (var plugin in _registry.GetAll())
        {
            var icon = plugin.MainIcon;
            icon.Size = new Size((int)(flowLayoutPanel1.Width * 0.8),
                                 (int)(flowLayoutPanel1.Width * 0.8));
            flowLayoutPanel1.Controls.Add(icon);
            plugin.Click += PluginControl_Click;
            plugin.MouseHover += PluginControl_Hover;

            if (plugin is IProvider provider)
            {
                _logger.LogDebug($"[1] Plugin {plugin.Name} implements {nameof(IProvider)}");
                provider.DataReceived += PluginControl_DataReceived;
            }
        }
    }

    private void HandleFormClosing(object sender, FormClosingEventArgs e)
    {
        foreach (var plugin in _registry.GetAll())
        {
            plugin.Click -= PluginControl_Click;
            plugin.MouseHover -= PluginControl_Hover;

            if (plugin is IProvider provider)
            {
                provider.DataReceived -= PluginControl_DataReceived;
            }

        }
    }

    private void HandleFormClosed(object sender, FormClosedEventArgs e)
    {
        Debug.WriteLine("Form Closed");
    }
}