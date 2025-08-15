using System.Diagnostics;
using BroadcastPluginSDK;

namespace Broadcast.SubForms;

public partial class MainForm : Form
{
    public delegate IEnumerable<KeyValuePair<string, string>> CacheReader(List<string> values);

    private readonly StartUp _plugins;

    public MainForm(StartUp plugins)
    {
        _plugins = plugins;
        InitializeComponent();
        toolStripStatusLabel.Text = Strings.PluginStarting;

        _plugins.AttachTo(this);

        _plugins.AttachMasterReader();
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