using Broadcast.Classes;
using BroadcastPluginSDK.Classes;
using BroadcastPluginSDK.Interfaces;
using Microsoft.Extensions.Logging;
using System.Drawing.Text;

namespace Broadcast.SubForms;

public partial class UpdateForm : Form
{
    private IPluginUpdater _updates;
    private ILogger<IPlugin> _logger;
    private readonly PluginCardRenderer _renderer = new();

    private async void UpdateForm_Load(object? sender, EventArgs e)
    {
        await webView21.EnsureCoreWebView2Async();
        var result = _updates.Releases;

        bool first = true;
        foreach (var release in result)
        {
            if (first)
            {
                webView21.NavigateToString(release.ReadMe);
                first = false;
            }

            listBox1.Items.Add(release);
        }
    }

//    private async void ListBox1_SelectedIndexChanged(object? sender, EventArgs e)
    private void ListBox1_SelectedIndexChanged(object? sender, EventArgs e)
    {

        if (listBox1.SelectedItem is ReleaseListItem selected)
        {
            if (!string.IsNullOrWhiteSpace(selected.ReadMe))
            {
                webView21.NavigateToString(selected.ReadMe);
            }
            else
            {
                webView21.NavigateToString("<p>README not found or failed to load.</p>");
            }
        }
    }

    private void CloseForm(object sender, MouseEventArgs e)
    {
        Close();
    }

    public UpdateForm(ILogger<IPlugin> logger, IPluginUpdater updates)
    {
        _updates = updates;
        _logger = logger;

        _logger.LogInformation("Starting Update Form");
        InitializeComponent();

        listBox1.DrawMode = DrawMode.OwnerDrawFixed;
        listBox1.ItemHeight = 72; // Slightly more breathing room
        listBox1.Font = new Font("Segoe UI", 12);
        listBox1.BackColor = Color.White;
        listBox1.BorderStyle = BorderStyle.None;

        listBox1.Dock = DockStyle.Fill;
        listBox1.Margin = new Padding(0);
        // Events
        listBox1.DrawItem += ListBox1_DrawItem;
        listBox1.MeasureItem += ListBox1_MeasureItem;
        listBox1.SelectedIndexChanged += ListBox1_SelectedIndexChanged;
        this.Load += UpdateForm_Load;
    }
    private void ListBox1_MeasureItem(object? sender, MeasureItemEventArgs e)
    {
        e.ItemHeight = 88; // Or calculate based on content
    }

    private void ListBox1_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= listBox1.Items.Count) return;

        var item = (ReleaseListItem)listBox1.Items[e.Index];
        _renderer.Draw(e.Graphics, e.Bounds, item, (e.State & DrawItemState.Selected) != 0);

        e.DrawFocusRectangle();
    }
}