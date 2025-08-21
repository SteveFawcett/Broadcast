using Broadcast.Classes;
using BroadcastPluginSDK.Classes;
using BroadcastPluginSDK.Interfaces;
using Markdig;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Drawing.Text;
using System.Text.RegularExpressions;

namespace Broadcast.SubForms;

public partial class UpdateForm : Form
{
    private IPluginUpdater _updates;
    private ILogger<IPlugin> _logger;
    private readonly PluginCardRenderer _renderer = new();

    private void DisplayLink(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            _logger.LogWarning("No URL provided to display link.");
            return;
        }
        LinkLabel.Link link = new LinkLabel.Link();
        link.LinkData = url;
        link.Start = 0;
        link.Length = linkLabel1.Text.Length;

        linkLabel1.Links.Clear();
        linkLabel1.Tag = url;
        linkLabel1.Links.Add( link);
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        if (e.Link?.LinkData is not string url || string.IsNullOrWhiteSpace(url))
        {
            _logger.LogWarning("Link data is null or not a valid URL string.");
            return;
        }

        if (Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
            (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
        {
            _logger.LogInformation("Opening link: {Link}", url);
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open link: {Link}", url);
            }
        }
        else
        {
            _logger.LogWarning("Link data is not a valid absolute HTTP/HTTPS URL.");
        }
    }


    private void UpdateForm_Load(object? sender, EventArgs e)
    {
        var result = _updates.Releases;

        bool first = true;
        foreach (var release in result)
        {
            if (first)
            {
                richTextBox1.Rtf = MarkdownToRtfConverter.Convert(release.ReadMe);
                DisplayLink(release.ReadMeDocUrl);
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
                richTextBox1.Rtf = MarkdownToRtfConverter.Convert(selected.ReadMe);
                DisplayLink(selected.ReadMeDocUrl);
            }
            else
            {
                richTextBox1.Rtf = MarkdownToRtfConverter.Convert( "# README not found or failed to load");
                DisplayLink(selected.Repo);
            }
        }
        else
        {
            _logger.LogWarning("Selected item is not a ReleaseListItem");
            richTextBox1.Rtf = MarkdownToRtfConverter.Convert("# README not found or failed to load");
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


    }
    private void ListBox1_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= listBox1.Items.Count) return;

        var item = (ReleaseListItem)listBox1.Items[e.Index];
        _renderer.Draw(e.Graphics, e.Bounds, item, (e.State & DrawItemState.Selected) != 0);

        e.DrawFocusRectangle();
    }
}