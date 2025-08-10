using Microsoft.Extensions.Configuration;
using PluginBase;
using System.Diagnostics;
using System.Windows.Forms;

namespace Broadcast
{
    public partial class MainForm : Form
    {
        StartUp Plugins ;

        public MainForm(IConfigurationRoot Configuration , StartUp plugins )
        {
            Plugins = plugins;
            InitializeComponent();
            toolStripStatusLabel.Text = "Plugins Starting";
            Plugins.AttachTo(this);
            toolStripStatusLabel.Text = "System Started";
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
            if(sender is IPlugin c)
            {
                toolStripStatusLabel.Text = $"{c.Name} ({c.Version}) : {c.Description}";
            }
        }

        internal void PluginControl_DataReceived(object? sender, PluginData e)
        {
            foreach (ICache plugin in Plugins.Caches())
            {
                plugin.Write(e);
            }
        }
    }
}
