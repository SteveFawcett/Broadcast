using BroadcastPluginSDK;
using BroadcastPluginSDK.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Broadcast.SubForms;

public partial class MainForm : Form
{
    public delegate IEnumerable<KeyValuePair<string, string>> CacheReader(List<string> values);

    private readonly ILogger<IPlugin> _logger;
    private readonly IPluginRegistry _registry;
    private readonly IConfiguration _configuration;
    ContextMenuStrip contextMenu = new ContextMenuStrip();

    public ILogger Logger => _logger;

    // Win32 API for dragging custom title bar

    public MainForm(IConfiguration configuration, ILogger<IPlugin> logger, IPluginRegistry registry)
    {
        _configuration = configuration;
        _logger = logger;
        _registry = registry;

        InitializeComponent();
        AttachPlugins();
    }

    // 🧩 Plugin UI Setup
    public void AttachPlugins()
    {
        foreach (IPlugin plugin in _registry.GetAll())
        {
            var container = CreatePluginIconContainer(plugin.ShortName, plugin.MainIcon);
            flowLayoutPanel1.Controls.Add(container);

            _logger.LogInformation("Attaching plugin {Name}", plugin.Name);
            plugin.Click += PluginControl_Click;
            

            if (plugin is IProvider provider)
            {
                _logger.LogDebug("Plugin {Name} implements {provider}", plugin.Name, nameof(IProvider));
                provider.DataReceived += PluginControl_DataReceived;
            }

            if (plugin is IManager manager)
            {
                _logger.LogDebug("Plugin {Name} implements {Interface}", plugin.Name, nameof(IManager));
                manager.TriggerRestart += PluginControl_Restart;
                manager.ShowScreen += Plugin_ShowScreen;
            }
        }
    }

    private void Plugin_ShowScreen(object? sender, UserControl e)
    {
        _logger.LogInformation("Plugin {plugin} requested to show custom screen", sender?.GetType().Name);

        panel.Controls.Clear();
        panel.Controls.Add(e);
        Width = e.Width + flowLayoutPanel1.Width + 50;  
    }

    private void PluginControl_Restart(object? sender, bool e)
    {
        if ( _registry.Restart || e  )
        {
            _logger.LogInformation("Restarting application as requested by plugin {plugin}", sender?.GetType().Name);
            Application.Restart();
            Environment.Exit(0);
        }
    }

    private Panel CreatePluginIconContainer(string name, BroadcastPluginSDK.Classes.MainIcon icon)
    {
        icon.Size = new Size((int)(flowLayoutPanel1.Width * 0.65),
                             (int)(flowLayoutPanel1.Width * 0.65));
        icon.Location = new Point(5, 5);

        var container = new Panel
        {
            Size = new Size(icon.Width + 10, icon.Height + 30),
            Margin = new Padding(5),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };
        container.Controls.Add(icon);

        var label = new Label
        {
            Text = name,
            Dock = DockStyle.Bottom,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 6, FontStyle.Regular)
        };
        container.Controls.Add(label);

        container.MouseEnter += (s, e) => container.BackColor = Color.LightSteelBlue;
        container.MouseLeave += (s, e) => container.BackColor = Color.White;

        return container;
    }

    // 🧠 Plugin Event Handlers
    public void PluginControl_Click(object? sender, MouseEventArgs e)
    { 
        MouseEventArgs me = e ?? new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);
        Logger.LogDebug("PluginControl_Click type: {type} arguments: {arguments} Button: {button}", sender?.GetType().Name, e.GetType().Name , me.Button);

        if (me.Button == MouseButtons.Left)
        {
            if (sender is IPlugin plugin)
            {
                panel.Controls.Clear();
                var page = plugin.InfoPage.GetControl();
                panel.Controls.Add(page);
            }
        }
        else if (me.Button == MouseButtons.Right)
        {
            if (sender is IManager plugin)
            {
                contextMenu.Items.Clear();

                var menuItems = plugin.ContextMenuItems;

                if (menuItems != null && menuItems.Any())
                {
                    contextMenu.Items.AddRange(menuItems.ToArray());
                    contextMenu.Show(Cursor.Position);
                }
            }
        }
    }

    internal void PluginControl_DataReceived(object? sender, Dictionary<string, string> e)
    {
        foreach (var plugin in _registry.Caches() ?? [])
        {
            if (plugin is ICache cache)
                cache.Write(e);
        }
    }

    // 🧹 Cleanup
    private void HandleFormClosing(object sender, FormClosingEventArgs e)
    {
        foreach (var plugin in _registry.GetAll())
        {
            plugin.Click -= PluginControl_Click;

            if (plugin is IProvider provider)
                provider.DataReceived -= PluginControl_DataReceived;
        }
    }

    private void panel_ControlAdded(object sender, ControlEventArgs e)
    {
        Panel? p = sender as Panel;
        if (p != null && p.Controls.Count > 0)
        {
            var ctrl = p.Controls[0];
  
            p.Size = ctrl.Size;

            if ( this.Width < p.Width + flowLayoutPanel1.Width + 50 )
            {
                this.Width = p.Width + flowLayoutPanel1.Width + 50;

            }
        }
    }
}
