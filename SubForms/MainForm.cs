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
    private readonly IPluginRegistry _registry;

    public ILogger Logger => _logger;
    public IConfiguration Configuration => _configuration;

    public MainForm(IConfiguration configuration, ILogger<MainForm> logger  , IPluginRegistry registry )
    {
        _configuration = configuration;
        _logger = logger;
        _registry = registry;

        InitializeComponent();
        toolStripStatusLabel.Text = Strings.PluginStarting;
        
        AttachToForm();

        //_startup.AttachMasterReader();
    }


    // Update the code in MainForm to use the new method:
    public void PluginControl_Click(object? sender, EventArgs e)
    {
        Logger.LogDebug($"PluginControl_Click {sender?.GetType().Name}");
        if (sender is IPlugin c)
        {
            panel.Controls.Clear();
            if (c.InfoPage is not null)
            {
                panel.Controls.Add(c.InfoPage.GetControl()); // Use the GetControl method to add the Control.
            }
        }
    }

    internal void PluginControl_Hover(object? sender, EventArgs e)
    {
        //if (sender is IPlugin c) toolStripStatusLabel.Text = $"{c.Name} ({c.Version}) : {c.Description}";
    }

    internal void PluginControl_DataReceived(object? sender, Dictionary<string, string> e)
    {
       // foreach (var plugin in _startup.Caches() ?? [])
       // {
       //     if (plugin is not ICache c) continue;
       //     c.Write(e);
       // }
    }

    private void CheckForUpdates(object sender, EventArgs e)
    {
        //Logger.LogDebug("Checking for updates...");
       // UpdateForm updateForm = new(_startup.All());
        //updateForm.ShowDialog(this);
    }

    public void AttachToForm( )
    {
        foreach( var plugin in _registry.GetAll() )
        {
            //Debug.WriteLine($"Attaching plugin {plugin.Name} to form");
            //if (plugin.InfoPage == null)
           // {
           //     Debug.WriteLine($"Plugin {plugin.Name} does not have an InfoPage set.");
           // }
            flowLayoutPanel1.Controls.Add(plugin.MainIcon);
            plugin.Click += PluginControl_Click;
            plugin.MouseHover += PluginControl_Hover;

            if (plugin is IProvider provider)
            {
                //Debug.WriteLine($"[1] Plugin {plugin.Name} implements {nameof(IProvider)}");
                provider.DataReceived += PluginControl_DataReceived;
                
                //if (plugin is ICache cache) Debug.WriteLine($"[2] Plugin {plugin.Name} implements {nameof(ICache)}");
            }
        }
    }
}